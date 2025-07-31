using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Nodes;
using Newtonsoft.Json;

namespace Utilities.Utils.Json
{
    public static class JsonHelper
    {
        private static readonly JsonSerializerOptions _options = new()
        {
            WriteIndented = true,
            PropertyNameCaseInsensitive = true
        };

        /// <summary>
        /// Ensures a JsonNode is properly initialized
        /// </summary>
        private static JsonObject EnsureInitialized(JsonNode jsonNode)
        {
            if (jsonNode == null)
            {
                throw new ArgumentNullException(nameof(jsonNode), "JSON node cannot be null");
            }

            if (jsonNode is not JsonObject jsonObject)
            {
                throw new ArgumentException("JSON node must be a JsonObject", nameof(jsonNode));
            }

            return jsonObject;
        }

        /// <summary>
        /// Reads and parses a JSON file using System.Text.Json
        /// </summary>
        public static JsonNode ReadJsonFile(string filePath)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"JSON file not found: {filePath}");
            }

            string jsonContent = File.ReadAllText(filePath);
            return JsonNode.Parse(jsonContent) ?? throw new InvalidOperationException("Failed to parse JSON file.");
        }

        /// <summary>
        /// Writes a JsonNode object to a file using System.Text.Json
        /// </summary>
        public static void WriteJsonFile(string filePath, JsonNode jsonNode)
        {
            var jsonObject = EnsureInitialized(jsonNode);
            string jsonContent = jsonObject.ToJsonString(_options);
            File.WriteAllText(filePath, jsonContent);
        }

        /// <summary>
        /// Gets a value from a JSON node by path
        /// </summary>
        public static T GetValue<T>(JsonNode jsonNode, string path)
        {
            var jsonObject = EnsureInitialized(jsonNode);
            JsonNode currentNode = jsonObject;
            string[] pathParts = path.Split('.');

            foreach (string part in pathParts)
            {
                currentNode = currentNode[part] ?? throw new ArgumentException($"Path '{path}' not found in JSON.");
            }

            return currentNode.GetValue<T>();
        }

        /// <summary>
        /// Sets a value in a JSON node by path
        /// </summary>
        public static void SetValue<T>(JsonNode jsonNode, string path, T value)
        {
            var jsonObject = EnsureInitialized(jsonNode);
            JsonNode currentNode = jsonObject;
            string[] pathParts = path.Split('.');

            for (int i = 0; i < pathParts.Length - 1; i++)
            {
                string part = pathParts[i];
                if (currentNode[part] == null)
                {
                    currentNode[part] = new JsonObject();
                }
                currentNode = currentNode[part];
            }

            string lastPart = pathParts[^1];
            currentNode[lastPart] = JsonValue.Create(value);
        }

        /// <summary>
        /// Adds a new section to the JSON
        /// </summary>
        public static void AddSection(JsonNode jsonNode, string sectionName, JsonObject sectionContent)
        {
            var jsonObject = EnsureInitialized(jsonNode);
            if (jsonObject[sectionName] != null)
            {
                throw new ArgumentException($"Section '{sectionName}' already exists in JSON.");
            }

            jsonObject[sectionName] = sectionContent ?? throw new ArgumentNullException(nameof(sectionContent));
        }

        /// <summary>
        /// Removes a section from the JSON
        /// </summary>
        public static void RemoveSection(JsonNode jsonNode, string path)
        {
            var jsonObject = EnsureInitialized(jsonNode);
            JsonNode currentNode = jsonObject;
            string[] pathParts = path.Split('.');

            for (int i = 0; i < pathParts.Length - 1; i++)
            {
                string part = pathParts[i];
                currentNode = currentNode[part] ?? throw new ArgumentException($"Path '{path}' not found in JSON.");
            }

            string lastPart = pathParts[^1];
            if (currentNode is JsonObject currentObject)
            {
                currentObject.Remove(lastPart);
            }
            else
            {
                throw new InvalidOperationException("Cannot remove from a non-object node.");
            }
        }

        /// <summary>
        /// Gets the connection string from the JSON configuration
        /// </summary>
        public static string GetConnectionString(JsonNode jsonNode, string connectionName = "DefaultConnection")
        {
            EnsureInitialized(jsonNode);
            return GetValue<string>(jsonNode, $"ConnectionStrings.{connectionName}");
        }

        /// <summary>
        /// Sets the connection string in the JSON configuration
        /// </summary>
        public static void SetConnectionString(JsonNode jsonNode, string connectionString, string connectionName = "DefaultConnection")
        {
            EnsureInitialized(jsonNode);
            SetValue(jsonNode, $"ConnectionStrings.{connectionName}", connectionString);
        }

        /// <summary>
        /// Gets an application setting from the JSON configuration
        /// </summary>
        public static string GetApplicationSetting(JsonNode jsonNode, string settingName)
        {
            EnsureInitialized(jsonNode);
            return GetValue<string>(jsonNode, $"ApplicationSettings.{settingName}");
        }

        /// <summary>
        /// Sets an application setting in the JSON configuration
        /// </summary>
        public static void SetApplicationSetting(JsonNode jsonNode, string settingName, string value)
        {
            EnsureInitialized(jsonNode);
            SetValue(jsonNode, $"ApplicationSettings.{settingName}", value);
        }

        /// <summary>
        /// Creates a new empty JsonObject
        /// </summary>
        public static JsonObject CreateNewJsonObject()
        {
            return new JsonObject();
        }

        /// <summary>
        /// Initializes a new JSON configuration with default structure
        /// </summary>
        public static JsonObject InitializeDefaultConfig()
        {
            return new JsonObject
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
        }

        /// <summary>
        /// Serializes an object to JSON string using Newtonsoft.Json
        /// </summary>
        public static string SerializeObject(object obj)
        {
            try
            {
                if (obj == null)
                {
                    return string.Empty;
                }

                return JsonConvert.SerializeObject(obj, Formatting.Indented);
            }
            catch (Exception ex)
            {
                throw new JsonSerializationException("Failed to serialize object to JSON", ex);
            }
        }

        /// <summary>
        /// Deserializes JSON string to object using Newtonsoft.Json
        /// </summary>
        public static T DeserializeObject<T>(string json)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(json))
                {
                    return default;
                }

                return JsonConvert.DeserializeObject<T>(json);
            }
            catch (Exception ex)
            {
                throw new JsonSerializationException($"Failed to deserialize JSON to type {typeof(T).Name}", ex);
            }
        }

        /// <summary>
        /// Deep clones a JSON object using serialization
        /// </summary>
        public static JsonNode DeepClone(JsonNode jsonNode)
        {
            var jsonString = jsonNode?.ToJsonString(_options) ?? throw new ArgumentNullException(nameof(jsonNode));
            return JsonNode.Parse(jsonString) ?? throw new InvalidOperationException("Failed to clone JSON node");
        }
    }
}