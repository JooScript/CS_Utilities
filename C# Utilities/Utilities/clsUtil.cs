namespace Utilities
{
    public class clsUtil
    {
        public static string GenerateGUID()
        {
            return Guid.NewGuid().ToString();
        }

        public static void ErrorLogger(Exception ex)
        {
            clsFile.LogLevel = clsFile.enLogLevel.Error;
            clsLogger FileLogger = new clsLogger(clsFile.LogToFile);
            FileLogger.Log(clsFormat.ExceptionToString(ex));
        }

        public static bool CreateFolderIfDoesNotExist(string FolderPath)
        {
            if (!Directory.Exists(FolderPath))
            {
                try
                {
                    Directory.CreateDirectory(FolderPath);
                    return true;
                }
                catch (Exception ex)
                {
                    clsUtil.ErrorLogger(ex);
                    return false;
                }
            }
            return true;
        }

        public static string ConvertDbTypeToCSharpType(string dbDataType)
        {
            // Handle nullable types by checking if the input ends with "?"
            bool isNullable = dbDataType.EndsWith("?");
            string typeWithoutNullable = isNullable ? dbDataType.Substring(0, dbDataType.Length - 1) : dbDataType;

            // Convert SQL Server data types to C# types
            string csharpType = typeWithoutNullable.ToLower() switch
            {
                // Exact matches
                "int" => "int",
                "bigint" => "long",
                "smallint" => "short",
                "tinyint" => "byte",
                "bit" => "bool",
                "float" => "double",
                "real" => "float",
                "decimal" or "numeric" or "money" or "smallmoney" => "decimal",
                "datetime" or "datetime2" or "smalldatetime" => "DateTime",
                "date" => "DateTime",
                "time" => "TimeSpan",
                "datetimeoffset" => "DateTimeOffset",
                "char" or "varchar" or "text" or "nchar" or "nvarchar" or "ntext" => "string",
                "binary" or "varbinary" or "image" => "byte[]",
                "uniqueidentifier" => "Guid",
                "xml" => "string",
                "sql_variant" => "object",
                "timestamp" or "rowversion" => "byte[]",

                // Handle common aliases
                "integer" => "int",
                "boolean" => "bool",
                "double precision" => "double",
                "character varying" => "string",
                "national char" or "national character" => "string",
                "national varchar" => "string",

                // Default case
                _ => "object" // fallback for unknown types
            };

            // Add nullable modifier if needed (except for string and byte[] which are already reference types)
            if (isNullable && csharpType != "string" && csharpType != "object" && !csharpType.EndsWith("[]"))
            {
                csharpType += "?";
            }

            return csharpType;
        }

    }
}
