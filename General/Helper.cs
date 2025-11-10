using Microsoft.AspNetCore.Http;
using System.Data;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using Utils.FileActions;

namespace Utils.General;

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
        if (String.IsNullOrEmpty(FolderPath))
        {
            return false;
        }

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

    public static DataTable ToDataTable<T>(List<T> items)
    {
        DataTable dataTable = new DataTable(typeof(T).Name);

        // Get all the properties of T
        PropertyInfo[] props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

        // Add columns to DataTable
        foreach (var prop in props)
        {
            dataTable.Columns.Add(prop.Name, Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType);
        }

        // Add rows
        foreach (T item in items)
        {
            var values = new object[props.Length];
            for (int i = 0; i < props.Length; i++)
            {
                values[i] = props[i].GetValue(item, null) ?? DBNull.Value;
            }
            dataTable.Rows.Add(values);
        }

        return dataTable;
    }

    /// <summary>
    /// Cleans a string into a URL-safe slug (e.g. for filenames, SEO URLs, etc.).
    /// </summary>
    /// <param name="input">The text to clean.</param>
    /// <returns>A cleaned, lowercase, hyphen-separated string.</returns>
    public static string CleanToSlug(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return string.Empty;

        // Normalize and convert to lowercase
        string slug = input.Trim().ToLowerInvariant();

        // Remove diacritics (e.g., café → cafe)
        slug = RemoveDiacritics(slug);

        // Replace non-alphanumeric characters with a single hyphen
        slug = Regex.Replace(slug, @"[^a-z0-9]+", "-");

        // Remove leading or trailing hyphens
        slug = Regex.Replace(slug, @"^-+|-+$", "");

        return slug;
    }

    private static string RemoveDiacritics(string text)
    {
        var normalized = text.Normalize(NormalizationForm.FormD);
        var builder = new StringBuilder();

        foreach (char c in normalized)
        {
            var unicodeCategory = System.Globalization.CharUnicodeInfo.GetUnicodeCategory(c);
            if (unicodeCategory != System.Globalization.UnicodeCategory.NonSpacingMark)
                builder.Append(c);
        }

        return builder.ToString().Normalize(NormalizationForm.FormC);
    }

}
