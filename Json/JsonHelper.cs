using System.Text.Json;
using System.Text.Json.Nodes;
using Newtonsoft.Json;

namespace Utils.Json;

/// <summary>
/// Provides helper methods for working with JSON configurations.
/// Supports both System.Text.Json and Newtonsoft.Json for serialization/deserialization.
/// </summary>
public static class JsonHelper
{
    private static readonly JsonSerializerOptions _options = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true
    };

    #region Private Helpers

    private static JsonObject EnsureInitialized(JsonNode node)
    {
        if (node is not JsonObject jsonObject)
            throw new ArgumentException("JSON node must be a JsonObject and cannot be null.", nameof(node));
        return jsonObject;
    }

    private static JsonObject NavigateToPath(JsonNode node, string path, bool createMissing = false)
    {
        var current = EnsureInitialized(node) as JsonObject
                      ?? throw new ArgumentException("Root node must be a JSON object.");

        var parts = path.Split('.', StringSplitOptions.RemoveEmptyEntries);

        for (int i = 0; i < parts.Length - 1; i++)
        {
            var part = parts[i];
            if (current[part] is null)
            {
                if (createMissing)
                    current[part] = new JsonObject();
                else
                    throw new ArgumentException($"Path '{path}' not found in JSON.");
            }

            current = current[part] as JsonObject
                      ?? throw new InvalidOperationException($"Path '{part}' is not a JSON object.");
        }

        return current;
    }


    #endregion

    #region File Operations

    public static JsonNode ReadJsonFile(string filePath)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException($"JSON file not found: {filePath}");

        var content = File.ReadAllText(filePath);
        return JsonNode.Parse(content) ?? throw new InvalidOperationException("Failed to parse JSON file.");
    }

    public static void WriteJsonFile(string filePath, JsonNode jsonNode)
    {
        File.WriteAllText(filePath, EnsureInitialized(jsonNode).ToJsonString(_options));
    }

    #endregion

    #region Value Access

    public static T GetValue<T>(JsonNode node, string path)
    {
        var current = NavigateToPath(node, path);
        var lastPart = path.Split('.').Last();

        var targetNode = current[lastPart];
        if (targetNode is null)
            throw new ArgumentException($"Value at '{path}' not found in JSON.");

        return targetNode.GetValue<T>();
    }

    public static void SetValue<T>(JsonNode node, string path, T value)
    {
        var current = NavigateToPath(node, path, createMissing: true);
        current[path.Split('.').Last()] = JsonValue.Create(value);
    }

    #endregion

    #region Section Operations

    public static void AddSection(JsonNode node, string sectionName, JsonObject content)
    {
        var jsonObject = EnsureInitialized(node);
        if (jsonObject.ContainsKey(sectionName))
            throw new ArgumentException($"Section '{sectionName}' already exists in JSON.");

        jsonObject[sectionName] = content ?? throw new ArgumentNullException(nameof(content));
    }

    public static void RemoveSection(JsonNode node, string path)
    {
        var parent = NavigateToPath(node, path);
        var lastPart = path.Split('.').Last();

        if (parent is JsonObject obj && obj.ContainsKey(lastPart))
            obj.Remove(lastPart);
        else
            throw new ArgumentException($"Section '{path}' not found or not removable.");
    }

    #endregion

    #region Configuration Shortcuts

    public static string GetConnectionString(JsonNode node, string name = "DefaultConnection") =>
        GetValue<string>(node, $"ConnectionStrings.{name}");

    public static void SetConnectionString(JsonNode node, string connection, string name = "DefaultConnection") =>
        SetValue(node, $"ConnectionStrings.{name}", connection);

    public static string GetApplicationSetting(JsonNode node, string setting) =>
        GetValue<string>(node, $"ApplicationSettings.{setting}");

    public static void SetApplicationSetting(JsonNode node, string setting, string value) =>
        SetValue(node, $"ApplicationSettings.{setting}", value);

    #endregion

    #region Creation

    public static JsonObject CreateNewJsonObject() => new();

    public static JsonObject InitializeDefaultConfig() => new()
    {
        ["ConnectionStrings"] = new JsonObject
        {
            ["DefaultConnection"] = ""
        },
        ["ApplicationSettings"] = new JsonObject
        {
            ["AppName"] = "",
            ["Version"] = "1.0.0",
            ["Environment"] = "Development"
        },
        ["Logging"] = new JsonObject
        {
            ["LogLevel"] = new JsonObject
            {
                ["Default"] = "Information",
                ["Microsoft.AspNetCore"] = "Warning"
            }
        },
        ["AllowedHosts"] = "*"
    };

    #endregion

    #region Serialization

    public static string SerializeObject(object obj)
    {
        if (obj == null) return string.Empty;

        try
        {
            return JsonConvert.SerializeObject(obj, Formatting.Indented);
        }
        catch (Exception ex)
        {
            throw new JsonSerializationException("Failed to serialize object to JSON.", ex);
        }
    }

    public static T DeserializeObject<T>(string json)
    {
        if (string.IsNullOrWhiteSpace(json)) return default;

        try
        {
            return JsonConvert.DeserializeObject<T>(json)!;
        }
        catch (Exception ex)
        {
            throw new JsonSerializationException($"Failed to deserialize JSON to type {typeof(T).Name}.", ex);
        }
    }

    public static JsonNode DeepClone(JsonNode node)
    {
        var json = node?.ToJsonString(_options)
                   ?? throw new ArgumentNullException(nameof(node));
        return JsonNode.Parse(json) ?? throw new InvalidOperationException("Failed to clone JSON node.");
    }

    #endregion
}
