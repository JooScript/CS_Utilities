using Microsoft.AspNetCore.Http;
using Utilities.FileActions;

namespace Utilities.Utils;

public static class Helper
{
    public static async Task<string> UploadImage(List<IFormFile> Files, string folderName)
    {
        foreach (var file in Files)
        {
            if (file.Length > 0)
            {
                string ImageName = GenerateGUID() + DateTime.Now.Year + DateTime.Now.Month + DateTime.Now.Day + ".jpg";
                var filePaths = Path.Combine(Directory.GetCurrentDirectory(), @"wwwroot\Uploads\" + folderName, ImageName);
                using (var stream = File.Create(filePaths))
                {
                    await file.CopyToAsync(stream);
                    return ImageName;
                }
            }
        }
        return string.Empty;
    }

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

    public static bool DeleteFolder(string folderPath, bool recursive = false)
    {
        if (string.IsNullOrWhiteSpace(folderPath))
        {
            FileHelper.ErrorLogger(new ArgumentNullException(nameof(folderPath), "Folder path cannot be null or whitespace."));
            return false;
        }

        try
        {
            if (!Directory.Exists(folderPath))
            {
                return true;
            }

            Directory.Delete(folderPath, recursive);
            return true;
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or ArgumentException or NotSupportedException)
        {
            FileHelper.ErrorLogger(ex);
            return false;
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
                FileHelper.ErrorLogger(ex);
                return false;
            }
        }
        return true;
    }

    public static string GetCSharpType(string sqlType)
    {
        bool isNullable = sqlType.EndsWith("?");
        string typeWithoutNullable = isNullable ? sqlType.Substring(0, sqlType.Length - 1) : sqlType;

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
            "date" => "DateOnly",
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
