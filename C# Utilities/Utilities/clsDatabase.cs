using Microsoft.Data.SqlClient;
using System.Collections.Concurrent;
using System.Data;
using System.Threading.Tasks;

namespace Utilities
{
    public static class clsDatabase
    {
        private static string _connectionString;
        private static readonly ConcurrentDictionary<string, TableSchema> _schemaCache = new();
        private static readonly object _lock = new();

        public static void Initialize(string connectionString)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new ArgumentException("Connection string cannot be null or empty", nameof(connectionString));
            }

            _connectionString = connectionString;
        }

        public static void ClearCache()
        {
            _schemaCache.Clear();
        }

        private static void _CheckConnectionStringInitialized()
        {
            if (string.IsNullOrEmpty(_connectionString))
            {
                throw new InvalidOperationException("Connection string has not been initialized. Call Initialize() first.");
            }
        }

        public static bool VerifyProcedureExists(SqlConnection connection, string procedureName)
        {
            if (connection == null)
            {
                throw new ArgumentNullException(nameof(connection));
            }

            if (string.IsNullOrWhiteSpace(procedureName))
            {
                throw new ArgumentException("Procedure name cannot be null or whitespace.", nameof(procedureName));
            }

            const string checkSql = @"
    SELECT 1 
    FROM sys.sql_modules m
    INNER JOIN sys.objects o ON m.object_id = o.object_id
    WHERE o.type = 'P' 
    AND SCHEMA_NAME(o.schema_id) = 'dbo' 
    AND o.name = @ProcedureName";

            try
            {
                using (var command = new SqlCommand(checkSql, connection))
                {
                    command.Parameters.Add("@ProcedureName", SqlDbType.NVarChar, 128).Value = procedureName;
                    command.CommandTimeout = 15;
                    return command.ExecuteScalar() != null;
                }
            }
            catch (SqlException ex)
            {
                clsUtil.ErrorLogger(ex);
                return false;
            }
            catch (Exception ex)
            {
                clsUtil.ErrorLogger(ex);
                return false;
            }
        }


        #region Async Methods

        public static async Task<List<string>> GetTableNamesAsync()
        {
            _CheckConnectionStringInitialized();

            var tables = new List<string>();

            try
            {
                await using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();

                const string query = @"
                    SELECT TABLE_NAME 
                    FROM INFORMATION_SCHEMA.TABLES 
                    WHERE TABLE_TYPE = 'BASE TABLE' 
                    AND TABLE_NAME NOT IN (
                        'sysdiagrams', 
                        '__EFMigrationsHistory',
                        'AspNetRoles',
                        'AspNetUsers',
                        'AspNetRoleClaims',
                        'AspNetUserClaims',
                        'AspNetUserLogins',
                        'AspNetUserRoles',
                        'AspNetUserTokens'
                    )
                    ORDER BY TABLE_NAME";

                await using var command = new SqlCommand(query, connection);
                await using var reader = await command.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    if (reader["TABLE_NAME"] is string tableName && !string.IsNullOrWhiteSpace(tableName))
                    {
                        tables.Add(tableName);
                    }
                }
            }
            catch (SqlException sqlEx)
            {
                clsUtil.ErrorLogger(sqlEx);
                throw;
            }
            catch (Exception ex)
            {
                clsUtil.ErrorLogger(ex);
                throw;
            }

            return tables;
        }

        public static async Task<List<ColumnInfo>> GetTableColumnsAsync(string tableName)
        {
            if (string.IsNullOrWhiteSpace(tableName))
            {
                throw new ArgumentException("Table name cannot be null or empty", nameof(tableName));
            }

            _CheckConnectionStringInitialized();

            var columns = new List<ColumnInfo>();

            try
            {
                await using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();

                const string query = @"
            SELECT 
                c.COLUMN_NAME, 
                c.DATA_TYPE, 
                c.IS_NULLABLE, 
                c.CHARACTER_MAXIMUM_LENGTH,
                c.NUMERIC_PRECISION,
                c.NUMERIC_SCALE,
                CASE WHEN pk.COLUMN_NAME IS NOT NULL THEN 1 ELSE 0 END AS IS_PRIMARY_KEY,
                CASE WHEN fk.COLUMN_NAME IS NOT NULL THEN 1 ELSE 0 END AS IS_FOREIGN_KEY,
                COLUMNPROPERTY(OBJECT_ID(c.TABLE_SCHEMA + '.' + c.TABLE_NAME), c.COLUMN_NAME, 'IsIdentity') AS IS_IDENTITY
            FROM INFORMATION_SCHEMA.COLUMNS c
            LEFT JOIN (
                SELECT COLUMN_NAME 
                FROM INFORMATION_SCHEMA.KEY_COLUMN_USAGE
                WHERE TABLE_NAME = @TableName
                AND CONSTRAINT_NAME LIKE 'PK_%'
            ) pk ON c.COLUMN_NAME = pk.COLUMN_NAME
            LEFT JOIN (
                SELECT COLUMN_NAME 
                FROM INFORMATION_SCHEMA.KEY_COLUMN_USAGE
                WHERE TABLE_NAME = @TableName
                AND CONSTRAINT_NAME LIKE 'FK_%'
            ) fk ON c.COLUMN_NAME = fk.COLUMN_NAME
            WHERE c.TABLE_NAME = @TableName
            ORDER BY c.ORDINAL_POSITION";

                await using var command = new SqlCommand(query, connection);
                command.Parameters.Add("@TableName", SqlDbType.NVarChar, 128).Value = tableName; // More specific parameter definition

                await using var reader = await command.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    columns.Add(new ColumnInfo
                    {
                        Name = reader["COLUMN_NAME"].ToString()!,
                        DataType = reader["DATA_TYPE"].ToString()!,
                        IsNullable = reader["IS_NULLABLE"].ToString() == "YES",
                        MaxLength = reader["CHARACTER_MAXIMUM_LENGTH"] as int?,
                        Precision = reader["NUMERIC_PRECISION"] as int?,
                        Scale = reader["NUMERIC_SCALE"] as int?,
                        IsPrimaryKey = Convert.ToBoolean(reader["IS_PRIMARY_KEY"]),
                        IsForeignKey = Convert.ToBoolean(reader["IS_FOREIGN_KEY"]),
                        IsIdentity = Convert.ToBoolean(reader["IS_IDENTITY"])
                    });
                }
            }
            catch (Exception ex)
            {
                clsUtil.ErrorLogger(ex);
                throw; // Consider wrapping in a custom exception
            }

            return columns;
        }

        public static async Task<List<string>> GetPrimaryKeysAsync(string tableName)
        {
            _CheckConnectionStringInitialized();

            var primaryKeys = new List<string>();

            try
            {
                await using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();

                const string query = @"
            SELECT COLUMN_NAME
            FROM INFORMATION_SCHEMA.KEY_COLUMN_USAGE kcu
            INNER JOIN INFORMATION_SCHEMA.TABLE_CONSTRAINTS tc
                ON kcu.CONSTRAINT_NAME = tc.CONSTRAINT_NAME
                AND kcu.TABLE_SCHEMA = tc.TABLE_SCHEMA
                AND kcu.TABLE_NAME = tc.TABLE_NAME
            WHERE kcu.TABLE_NAME = @TableName
            AND tc.CONSTRAINT_TYPE = 'PRIMARY KEY'
            ORDER BY kcu.ORDINAL_POSITION";

                await using var command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@TableName", tableName);

                await using var reader = await command.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    if (reader["COLUMN_NAME"] is string columnName)
                    {
                        primaryKeys.Add(columnName);
                    }
                }
            }
            catch (Exception ex)
            {
                clsUtil.ErrorLogger(ex);
                throw; // Consider whether to rethrow or return empty list
            }

            return primaryKeys;
        }

        public static async Task<string?> GetFirstPrimaryKeyAsync(string tableName)
        {
            var primaryKeys = await GetPrimaryKeysAsync(tableName);
            return primaryKeys?.FirstOrDefault();
        }

        public static async Task<List<ForeignKeyInfo>> GetForeignKeysAsync(string tableName)
        {
            _CheckConnectionStringInitialized();

            var foreignKeys = new List<ForeignKeyInfo>();

            try
            {
                await using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();

                const string query = @"
                    SELECT 
                        fk.COLUMN_NAME,
                        fk.REFERENCED_TABLE_NAME,
                        fk.REFERENCED_COLUMN_NAME,
                        rc.UPDATE_RULE,
                        rc.DELETE_RULE,
                        fk.CONSTRAINT_NAME
                    FROM INFORMATION_SCHEMA.KEY_COLUMN_USAGE fk
                    JOIN INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS rc
                        ON fk.CONSTRAINT_NAME = rc.CONSTRAINT_NAME
                    WHERE fk.TABLE_NAME = @TableName
                    AND fk.CONSTRAINT_NAME LIKE 'FK_%'";

                await using var command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@TableName", tableName);

                await using var reader = await command.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    foreignKeys.Add(new ForeignKeyInfo
                    {
                        ColumnName = reader["COLUMN_NAME"].ToString()!,
                        ReferencedTable = reader["REFERENCED_TABLE_NAME"].ToString()!,
                        ReferencedColumn = reader["REFERENCED_COLUMN_NAME"].ToString()!,
                        UpdateRule = reader["UPDATE_RULE"].ToString()!,
                        DeleteRule = reader["DELETE_RULE"].ToString()!,
                        ConstraintName = reader["CONSTRAINT_NAME"].ToString()!
                    });
                }
            }
            catch (Exception ex)
            {
                clsUtil.ErrorLogger(ex);
                throw;
            }

            return foreignKeys;
        }

        public static async Task<Dictionary<string, TableSchema>> GetDatabaseSchemaAsync(bool useCache = true)
        {
            _CheckConnectionStringInitialized();

            if (useCache && _schemaCache.Count > 0)
            {
                return _schemaCache.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
            }

            var schema = new Dictionary<string, TableSchema>();

            try
            {
                var tableNames = await GetTableNamesAsync();

                var tasks = tableNames.Select(async tableName =>
                {
                    var columnsTask = GetTableColumnsAsync(tableName);
                    var primaryKeysTask = GetPrimaryKeysAsync(tableName);
                    var foreignKeysTask = GetForeignKeysAsync(tableName);

                    await Task.WhenAll(columnsTask, primaryKeysTask, foreignKeysTask);

                    var tableSchema = new TableSchema
                    {
                        TableName = tableName,
                        Columns = await columnsTask,
                        PrimaryKeys = await primaryKeysTask,
                        ForeignKeys = await foreignKeysTask
                    };

                    lock (_lock)
                    {
                        schema[tableName] = tableSchema;
                        if (useCache)
                        {
                            _schemaCache[tableName] = tableSchema;
                        }
                    }
                });

                await Task.WhenAll(tasks);
            }
            catch (Exception ex)
            {
                clsUtil.ErrorLogger(ex);
                throw;
            }

            return schema;
        }

        #endregion

        #region Sync Methods

        public static List<string> GetTableNames()
        {
            return GetTableNamesAsync().GetAwaiter().GetResult();
        }

        public static List<ColumnInfo> GetTableColumns(string tableName)
        {
            return GetTableColumnsAsync(tableName).GetAwaiter().GetResult();
        }

        public static List<string> GetPrimaryKeys(string tableName)
        {
            return GetPrimaryKeysAsync(tableName).GetAwaiter().GetResult();
        }

        public static string GetFirstPrimaryKey(string tableName)
        {
            return GetFirstPrimaryKeyAsync(tableName).GetAwaiter().GetResult();
        }

        public static List<ForeignKeyInfo> GetForeignKeys(string tableName)
        {
            return GetForeignKeysAsync(tableName).GetAwaiter().GetResult();
        }

        public static Dictionary<string, TableSchema> GetDatabaseSchema(bool useCache = true)
        {
            return GetDatabaseSchemaAsync(useCache).GetAwaiter().GetResult();
        }

        #endregion

        #region Support Classes

        public class TableSchema
        {
            public string TableName { get; set; } = null!;
            public List<ColumnInfo> Columns { get; set; } = new();
            public List<string> PrimaryKeys { get; set; } = new();
            public List<ForeignKeyInfo> ForeignKeys { get; set; } = new();
            public bool HasPrimaryKey => PrimaryKeys.Count > 0;
            public bool HasForeignKeys => ForeignKeys.Count > 0;
        }

        public class ColumnInfo
        {
            public string Name { get; set; } = null!;
            public string DataType { get; set; } = null!;
            public bool IsNullable { get; set; }
            public int? MaxLength { get; set; }
            public int? Precision { get; set; }
            public int? Scale { get; set; }
            public bool IsPrimaryKey { get; set; }
            public bool IsForeignKey { get; set; }
            public bool IsIdentity { get; set; }
        }

        public class ForeignKeyInfo
        {
            public string ColumnName { get; set; } = null!;
            public string ReferencedTable { get; set; } = null!;
            public string ReferencedColumn { get; set; } = null!;
            public string UpdateRule { get; set; } = null!;
            public string DeleteRule { get; set; } = null!;
            public string ConstraintName { get; set; } = null!;
        }

        #endregion
    }
}