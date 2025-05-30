namespace Utilities
{
    public static class GeneralUtil
    {
        public static string GenerateGUID()
        {
            return Guid.NewGuid().ToString();
        }

        public static string GetMimeType(string filePath)
        {
            return Path.GetExtension(filePath).ToLowerInvariant() switch
            {
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".gif" => "image/gif",
                _ => "application/octet-stream",
            };
        }

        public static void ErrorLogger(Exception ex, bool exThrow = false)
        {
            FileUtil.LogLevel = FileUtil.enLogLevel.Error;
            LoggerUtil FileLogger = new LoggerUtil(FileUtil.LogToFile);
            FileLogger.Log(FormatUtil.ExceptionToString(ex));

            if (exThrow)
            {
                throw ex;
            }
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
                    GeneralUtil.ErrorLogger(ex);
                    return false;
                }
            }
            return true;
        }

        public static string ConvertDbTypeToCSharpType(string dbDataType)
        {
            bool isNullable = dbDataType.EndsWith("?");
            string typeWithoutNullable = isNullable ? dbDataType.Substring(0, dbDataType.Length - 1) : dbDataType;

            string csharpType = typeWithoutNullable.ToLower() switch
            {
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

        public static string GetDefaultValue(string csharpType, bool isNullable)
        {
            if (isNullable)
            {
                return "null";
            }

            switch (csharpType)
            {
                case "string": return "null";
                case "int": return "0";
                case "decimal": return "0m";
                case "double": return "0.0";
                case "float": return "0f";
                case "DateTime": return "DateTime.MinValue";
                case "bool": return "false";
                case "byte[]": return "null";
                case "byte": return "(byte)0";
                case "short": return "0";
                case "long": return "0L";
                default: return "null";
            }
        }

    }
}
