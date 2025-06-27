using Microsoft.Data.SqlClient;
using Microsoft.SqlServer.Management.Common;
using Microsoft.SqlServer.Management.Smo;
using System.Collections.Concurrent;
using System.Data;
using System.Text;

namespace Utilities
{
    public static class DatabaseHelper
    {
        public enum BackupType
        {
            Full = 0,
            Differential = 1
        }

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

        private static string _BuildMasterConnectionString()
        {
            SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder(_connectionString);
            builder.InitialCatalog = "master";
            return builder.ToString();
        }

        public static string ExtractAppNameFromConnectionString()
        {
            _CheckConnectionStringInitialized();

            if (string.IsNullOrWhiteSpace(_connectionString))
            {
                throw new ArgumentNullException(nameof(_connectionString), "Connection string cannot be null or empty");
            }

            try
            {
                var builder = new SqlConnectionStringBuilder(_connectionString);

                if (string.IsNullOrWhiteSpace(builder.InitialCatalog))
                {
                    throw new ArgumentException("Connection string does not contain a database name (Initial Catalog)");
                }

                return builder.InitialCatalog;
            }
            catch (ArgumentException ex)
            {
                Helper.ErrorLogger(new Exception("Invalid connection string format", ex));
                throw;
            }
            catch (Exception ex)
            {
                Helper.ErrorLogger(new Exception("Error extracting app name from connection string", ex));
                throw;
            }
        }

        public static string GetDefaultValueForType(string dbType, bool isNullable)
        {
            string csharpType = Helper.GetCSharpType(dbType);

            if (isNullable)
            {
                return "null";
            }

            switch (csharpType)
            {
                case "int":
                case "short":
                case "long":
                case "byte":
                    return "0";
                case "string":
                    return "string.Empty";
                case "bool":
                    return "false";
                case "DateTime":
                    return "DateTime.MinValue";
                case "decimal":
                case "double":
                case "float":
                    return "0";
                default:
                    return "null";
            }
        }

        /// <summary>
        /// Backs up a SQL Server database to the specified path.
        /// </summary>
        /// <param name="backupDirectory">The Directory of the backup file.</param>
        /// <param name="type">The type of backup (Full or Differential).</param>
        /// <returns>True if backup succeeded; false otherwise.</returns>
        /// <summary>
        /// Backs up a SQL Server database to a default backup directory with auto-generated file name.
        /// </summary>
        /// <param name="type">The type of backup (Full or Differential).</param>
        /// <returns>True if backup succeeded; false otherwise.</returns>
        public static bool BackupDatabase(ref string filePath, BackupType type = BackupType.Full)
        {
            _CheckConnectionStringInitialized();

            ServerConnection serverConnection = null;
            string backDir = $"C:\\BackupTemp";
            string databaseName = ExtractAppNameFromConnectionString();
            string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            string typeStr = type.ToString();
            string fileName = $"{databaseName}_{typeStr}_{timestamp}.bak";
            string backupPath = Path.Combine(backDir, fileName);

            try
            {

                if (!Directory.Exists(backDir))
                {
                    Directory.CreateDirectory(backDir);
                }

                using (SqlConnection sqlConnection = new SqlConnection(_connectionString))
                {
                    sqlConnection.Open();

                    serverConnection = new ServerConnection(sqlConnection);
                    Server server = new Server(serverConnection);

                    if (!server.Databases.Contains(databaseName))
                        throw new InvalidOperationException($"Database '{databaseName}' does not exist on server '{serverConnection.ServerInstance}'.");

                    Backup backup = new Backup
                    {
                        Action = BackupActionType.Database,
                        Database = databaseName,
                        BackupSetName = $"{databaseName} {typeStr} Backup",
                        BackupSetDescription = $"{typeStr} backup of {databaseName} on {DateTime.Now:yyyy-MM-dd HH:mm:ss}",
                        Incremental = type == BackupType.Differential,
                        LogTruncation = BackupTruncateLogType.Truncate,
                        Initialize = type == BackupType.Full
                    };

                    backup.Devices.AddDevice(backupPath, DeviceType.File);
                    backup.SqlBackup(server);
                }

                if (!File.Exists(backupPath))
                {
                    Helper.ErrorLogger(new IOException($"Backup file was not created at: {backupPath}"));
                }

            }
            catch (Exception ex)
            {
                Helper.ErrorLogger(new Exception("[ERROR] Database backup failed", ex), true);
                return false;
            }
            finally
            {
                if (serverConnection != null && serverConnection.IsOpen)
                {
                    serverConnection.Disconnect();
                }
            }

            FileHelper.WaitForFileAvailable(backupPath);
            FileHelper.MoveFileToFolder(filePath, ref backupPath);
            filePath = backupPath;
            Helper.DeleteFolder(backDir, true);
            return true;
        }

        /// <summary>
        /// Restores a SQL Server database from a backup file.
        /// </summary>
        /// <param name="backupFilePath">Full path to the .bak file</param>
        /// <param name="overwrite">True to overwrite existing database</param>
        /// <returns>True if restore succeeded, false otherwise</returns>
        public static bool RestoreDatabase(string backupFilePath, bool overwrite = true)
        {
            if (string.IsNullOrWhiteSpace(backupFilePath) || !File.Exists(backupFilePath))
            {
                Helper.ErrorLogger(new Exception($"[ERROR] Backup file not found: {backupFilePath}"));
                return false;
            }

            string databaseName = ExtractAppNameFromConnectionString();

            try
            {
                // Connect to the master database, not the target DB
                SqlConnection sqlConnection = new SqlConnection(_BuildMasterConnectionString());
                ServerConnection serverConnection = new ServerConnection(sqlConnection);
                Server sqlServer = new Server(serverConnection);

                // Ensure exclusive access to the DB before restore
                sqlServer.KillAllProcesses(databaseName);

                Restore restore = new Restore
                {
                    Action = RestoreActionType.Database,
                    Database = databaseName,
                    ReplaceDatabase = overwrite,
                    NoRecovery = false
                };
                restore.Devices.AddDevice(backupFilePath, DeviceType.File);
                restore.RelocateFiles.Clear();

                DataTable logicalFiles = restore.ReadFileList(sqlServer);
                string dataLogicalName = logicalFiles.Rows[0]["LogicalName"].ToString();
                string logLogicalName = logicalFiles.Rows[1]["LogicalName"].ToString();

                string dataFilePath = Path.Combine(sqlServer.Settings.DefaultFile, $"{databaseName}.mdf");
                string logFilePath = Path.Combine(sqlServer.Settings.DefaultLog, $"{databaseName}_log.ldf");

                restore.RelocateFiles.Add(new RelocateFile(dataLogicalName, dataFilePath));
                restore.RelocateFiles.Add(new RelocateFile(logLogicalName, logFilePath));

                restore.SqlRestore(sqlServer);
                return true;
            }
            catch (Exception ex)
            {
                Helper.ErrorLogger(new Exception("[ERROR] Database restore failed\n", ex), true);
                return false;
            }
        }

        #region Stored Precedures

        public static bool VerifyProcedureExists(string procedureName)
        {
            _CheckConnectionStringInitialized();

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
                using (SqlConnection connection = new SqlConnection(_connectionString))
                using (SqlCommand command = new SqlCommand(checkSql, connection))
                {
                    command.Parameters.Add("@ProcedureName", SqlDbType.NVarChar, 128).Value = procedureName;
                    command.CommandTimeout = 15;
                    connection.Open();
                    return command.ExecuteScalar() != null;
                }
            }
            catch (SqlException ex)
            {
                Helper.ErrorLogger(ex);
                return false;
            }
            catch (Exception ex)
            {
                Helper.ErrorLogger(ex);
                return false;
            }
        }

        public static bool CreateStoredProcedure(string procedureName, string procedureBody)
        {
            _CheckConnectionStringInitialized();
            string AppName = ExtractAppNameFromConnectionString();
            if (string.IsNullOrWhiteSpace(AppName))
                throw new ArgumentNullException(nameof(AppName));

            string procSql = $@"
USE [{AppName}];
GO

IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = '{procedureName}')
    DROP PROCEDURE [dbo].[{procedureName}];
GO

CREATE PROCEDURE [dbo].[{procedureName}]
{procedureBody}
GO";

            return DatabaseHelper.ExecuteProcedureCreation(procSql, procedureName);
        }

        public static bool ExecuteProcedureCreation(string procSql, string procedureName)
        {
            if (VerifyProcedureExists(procedureName))
            {
                if (!DeleteStoredProcedure(procedureName))
                {
                    return false;
                }
            }

            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    connection.Open();
                    var commands = procSql.Split(new[] { "GO" }, StringSplitOptions.RemoveEmptyEntries);

                    foreach (var cmdText in commands)
                    {
                        using (var command = new SqlCommand(cmdText, connection))
                        {
                            command.ExecuteNonQuery();
                        }
                    }

                    return VerifyProcedureExists(procedureName);
                }
            }
            catch (SqlException sqlEx)
            {
                Helper.ErrorLogger(new Exception(
                    $"Failed to create {procedureName} procedure. " +
                    $"SQL Error: {sqlEx.Message}", sqlEx));
                return false;
            }
            catch (Exception ex)
            {
                Helper.ErrorLogger(new Exception($"Unexpected error creating {procedureName} procedure. ", ex));
                return false;
            }
        }

        public static bool DeleteStoredProcedure(string procedureName)
        {
            _CheckConnectionStringInitialized();
            string DatabaseName = ExtractAppNameFromConnectionString();

            if (string.IsNullOrWhiteSpace(DatabaseName))
            {
                throw new ArgumentNullException(nameof(DatabaseName));
            }

            if (string.IsNullOrWhiteSpace(procedureName))
            {
                throw new ArgumentNullException(nameof(procedureName));
            }

            string dropSql = @"
IF EXISTS (SELECT * FROM sys.objects 
          WHERE type = 'P' AND name = @ProcedureName)
BEGIN
    DECLARE @sql NVARCHAR(MAX) = 'DROP PROCEDURE [dbo].[' + @ProcedureName + ']';
    EXEC sp_executesql @sql;
    SELECT 1 AS Result;
END
ELSE
BEGIN
    SELECT 0 AS Result;
END";

            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    connection.Open();

                    using (var useDbCommand = new SqlCommand($"USE [{DatabaseName}];", connection))
                    {
                        useDbCommand.ExecuteNonQuery();
                    }

                    using (var command = new SqlCommand(dropSql, connection))
                    {
                        command.Parameters.Add("@ProcedureName", SqlDbType.NVarChar, 128).Value = procedureName;
                        var result = (int)command.ExecuteScalar();
                        return result == 1;
                    }
                }
            }
            catch (SqlException sqlEx)
            {
                Helper.ErrorLogger(new Exception(
                    $"Failed to delete {procedureName} procedure. " +
                    $"Database: {DatabaseName}. " +
                    $"SQL Error: {sqlEx.Message}", sqlEx));
                return false;
            }
            catch (Exception ex)
            {
                Helper.ErrorLogger(new Exception(
                    $"Unexpected error deleting {procedureName} procedure. " +
                    $"Database: {DatabaseName}", ex));
                return false;
            }
        }

        /// <summary>
        /// Deletes all user-defined stored procedures in the database
        /// </summary>
        /// <param name="excludeSystemProcedures">Whether to exclude system stored procedures (default true)</param>
        /// <returns>Tuple containing (success flag, deleted count, error messages)</returns>
        public static (bool Success, int DeletedCount, List<string> Errors) DeleteAllStoredProcedures()
        {
            var errors = new List<string>();
            int deletedCount = 0;
            bool overallSuccess = true;
            bool excludeSystemProcedures = true;

            _CheckConnectionStringInitialized();

            if (string.IsNullOrWhiteSpace(_connectionString))
            {
                errors.Add("Connection string cannot be null or empty");
                return (false, 0, errors);
            }

            string databaseName = "";
            try
            {
                var builder = new SqlConnectionStringBuilder(_connectionString);
                databaseName = builder.InitialCatalog;

                if (string.IsNullOrWhiteSpace(databaseName))
                {
                    errors.Add("Connection string does not contain a database name (Initial Catalog)");
                    return (false, 0, errors);
                }
            }
            catch (Exception ex)
            {
                errors.Add($"Invalid connection string format: {ex.Message}");
                return (false, 0, errors);
            }

            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    connection.Open();

                    var sql = new StringBuilder(@"
                SELECT 
                    QUOTENAME(SCHEMA_NAME(schema_id)) + '.' + QUOTENAME(name) AS FullQualifiedProcedureName
                FROM sys.procedures
                WHERE type = 'P'");

                    if (excludeSystemProcedures)
                    {
                        sql.Append(" AND is_ms_shipped = 0");
                    }

                    var proceduresToDelete = new List<string>();
                    using (var command = new SqlCommand(sql.ToString(), connection))
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            proceduresToDelete.Add(reader["FullQualifiedProcedureName"].ToString());
                        }
                    }

                    // Delete procedures in reverse dependency order (optional - could be added)
                    foreach (var procedure in proceduresToDelete)
                    {
                        try
                        {
                            // Already properly quoted in the query
                            string dropSql = $"DROP PROCEDURE {procedure}";

                            using (var dropCommand = new SqlCommand(dropSql, connection))
                            {
                                dropCommand.ExecuteNonQuery();
                                deletedCount++;
                            }
                        }
                        catch (SqlException sqlEx)
                        {
                            overallSuccess = false;
                            errors.Add($"Failed to delete procedure {procedure}: {sqlEx.Message}");
                            // Log the specific error number for troubleshooting
                            errors.Add($"SQL Error #{sqlEx.Number}: {sqlEx.Message}");
                        }
                        catch (Exception ex)
                        {
                            overallSuccess = false;
                            errors.Add($"Unexpected error deleting procedure {procedure}: {ex.Message}");
                        }
                    }
                }
            }
            catch (SqlException sqlEx)
            {
                errors.Add($"Database error: {sqlEx.Message}");
                errors.Add($"SQL Error #{sqlEx.Number}: {sqlEx.Message}");
                return (false, deletedCount, errors);
            }
            catch (Exception ex)
            {
                errors.Add($"Unexpected error: {ex.Message}");
                return (false, deletedCount, errors);
            }

            return (overallSuccess, deletedCount, errors);
        }

        #endregion

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
                Helper.ErrorLogger(sqlEx);
                throw;
            }
            catch (Exception ex)
            {
                Helper.ErrorLogger(ex);
                throw;
            }

            return tables;
        }

        public static async Task<bool> TableExistsAsync(string tableName)
        {
            _CheckConnectionStringInitialized();
            if (string.IsNullOrWhiteSpace(tableName))
            {
                throw new ArgumentException("Table name cannot be null or empty", nameof(tableName));
            }

            var tableNames = await GetTableNamesAsync();
            return tableNames.Contains(tableName);
        }

        public static async Task<List<ColumnInfo>> GetTableColumnsAsync(string tableName)
        {
            if (string.IsNullOrWhiteSpace(tableName))
            {
                throw new ArgumentException("Table name cannot be null or empty", nameof(tableName));
            }

            _CheckConnectionStringInitialized();

            List<ColumnInfo> columns = new List<ColumnInfo>();

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
                command.Parameters.Add("@TableName", SqlDbType.NVarChar, 128).Value = tableName;
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
                Helper.ErrorLogger(ex, true);
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
                Helper.ErrorLogger(ex);
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
                col.name AS COLUMN_NAME,
                OBJECT_NAME(fk.referenced_object_id) AS REFERENCED_TABLE,
                COL_NAME(fk.referenced_object_id, fkc.referenced_column_id) AS REFERENCED_COLUMN,
                CASE fk.delete_referential_action
                    WHEN 0 THEN 'NO_ACTION'
                    WHEN 1 THEN 'CASCADE'
                    WHEN 2 THEN 'SET_NULL'
                    WHEN 3 THEN 'SET_DEFAULT'
                    ELSE 'NO_ACTION'
                END AS DELETE_RULE,
                CASE fk.update_referential_action
                    WHEN 0 THEN 'NO_ACTION'
                    WHEN 1 THEN 'CASCADE'
                    WHEN 2 THEN 'SET_NULL'
                    WHEN 3 THEN 'SET_DEFAULT'
                    ELSE 'NO_ACTION'
                END AS UPDATE_RULE,
                fk.name AS CONSTRAINT_NAME
            FROM sys.foreign_keys fk
            INNER JOIN sys.foreign_key_columns fkc 
                ON fk.object_id = fkc.constraint_object_id
            INNER JOIN sys.columns col 
                ON fkc.parent_object_id = col.object_id 
                AND fkc.parent_column_id = col.column_id
            WHERE OBJECT_NAME(fk.parent_object_id) = @TableName
            AND fk.name LIKE 'FK_%'";

                await using var command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@TableName", tableName);

                await using var reader = await command.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    foreignKeys.Add(new ForeignKeyInfo
                    {
                        ColumnName = reader["COLUMN_NAME"].ToString()!,
                        ReferencedTable = reader["REFERENCED_TABLE"].ToString()!,
                        ReferencedColumn = reader["REFERENCED_COLUMN"].ToString()!,
                        UpdateRule = reader["UPDATE_RULE"].ToString()!,
                        DeleteRule = reader["DELETE_RULE"].ToString()!,
                        ConstraintName = reader["CONSTRAINT_NAME"].ToString()!
                    });
                }
            }
            catch (Exception ex)
            {
                Helper.ErrorLogger(ex);
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
                Helper.ErrorLogger(ex);
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

        public static bool TableExists(string tableName)
        {
            return TableExistsAsync(tableName).GetAwaiter().GetResult();
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
            public string TableName
            {
                get;
                set;
            } = null!;
            public List<ColumnInfo> Columns
            {
                get;
                set;
            } = new();
            public List<string> PrimaryKeys
            {
                get;
                set;
            } = new();
            public List<ForeignKeyInfo> ForeignKeys
            {
                get;
                set;
            } = new();
            public bool HasPrimaryKey => PrimaryKeys.Count > 0;
            public bool HasForeignKeys => ForeignKeys.Count > 0;
        }

        public class ColumnInfo
        {
            public string Name
            {
                get;
                set;
            } = null!;
            public string DataType
            {
                get;
                set;
            } = null!;
            public bool IsNullable
            {
                get;
                set;
            }
            public int? MaxLength
            {
                get;
                set;
            }
            public int? Precision
            {
                get;
                set;
            }
            public int? Scale
            {
                get;
                set;
            }
            public bool IsPrimaryKey
            {
                get;
                set;
            }
            public bool IsForeignKey
            {
                get;
                set;
            }
            public bool IsIdentity
            {
                get;
                set;
            }
        }

        public class ForeignKeyInfo
        {
            public string ColumnName
            {
                get;
                set;
            } = null!;
            public string ReferencedTable
            {
                get;
                set;
            } = null!;
            public string ReferencedColumn
            {
                get;
                set;
            } = null!;
            public string UpdateRule
            {
                get;
                set;
            } = null!;
            public string DeleteRule
            {
                get;
                set;
            } = null!;
            public string ConstraintName
            {
                get;
                set;
            } = null!;
        }

        #endregion

    }
}