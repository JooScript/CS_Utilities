using Newtonsoft.Json;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Utilities
{
    public static class JsonHelper
    {
        private static readonly JsonSerializerOptions _options = new()
        {
            WriteIndented = true,
            PropertyNameCaseInsensitive = true
        };

        /// <summary>
        /// Reads and parses a JSON file
        /// </summary>
        /// <param name="filePath">Path to the JSON file</param>
        /// <returns>JsonNode object representing the JSON structure</returns>
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
        /// Writes a JsonNode object to a file
        /// </summary>
        /// <param name="filePath">Path to save the JSON file</param>
        /// <param name="jsonNode">JsonNode object to save</param>
        public static void WriteJsonFile(string filePath, JsonNode jsonNode)
        {
            string jsonContent = jsonNode.ToJsonString(_options);
            File.WriteAllText(filePath, jsonContent);
        }

        /// <summary>
        /// Gets a value from a JSON node by path (e.g., "ConnectionStrings.DefaultConnection")
        /// </summary>
        /// <typeparam name="T">Type of the value to return</typeparam>
        /// <param name="jsonNode">Root JsonNode</param>
        /// <param name="path">Dot-separated path to the property</param>
        /// <returns>Value of the specified property</returns>
        public static T GetValue<T>(JsonNode jsonNode, string path)
        {
            JsonNode currentNode = jsonNode;
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
        /// <typeparam name="T">Type of the value to set</typeparam>
        /// <param name="jsonNode">Root JsonNode</param>
        /// <param name="path">Dot-separated path to the property</param>
        /// <param name="value">Value to set</param>
        public static void SetValue<T>(JsonNode jsonNode, string path, T value)
        {
            JsonNode currentNode = jsonNode;
            string[] pathParts = path.Split('.');

            // Navigate to the parent of the target node
            for (int i = 0; i < pathParts.Length - 1; i++)
            {
                string part = pathParts[i];
                if (currentNode[part] == null)
                {
                    currentNode[part] = new JsonObject();
                }
                currentNode = currentNode[part];
            }

            // Set the value on the last part
            string lastPart = pathParts[^1];
            currentNode[lastPart] = JsonValue.Create(value);
        }

        /// <summary>
        /// Adds a new section to the JSON
        /// </summary>
        /// <param name="jsonNode">Root JsonNode</param>
        /// <param name="sectionName">Name of the new section</param>
        /// <param name="sectionContent">Content of the new section as a JsonObject</param>
        public static void AddSection(JsonNode jsonNode, string sectionName, JsonObject sectionContent)
        {
            if (jsonNode[sectionName] != null)
            {
                throw new ArgumentException($"Section '{sectionName}' already exists in JSON.");
            }

            jsonNode[sectionName] = sectionContent;
        }

        /// <summary>
        /// Removes a section from the JSON
        /// </summary>
        /// <param name="jsonNode">Root JsonNode</param>
        /// <param name="path">Dot-separated path to the section to remove</param>
        public static void RemoveSection(JsonNode jsonNode, string path)
        {
            JsonNode currentNode = jsonNode;
            string[] pathParts = path.Split('.');

            // Navigate to the parent of the target node
            for (int i = 0; i < pathParts.Length - 1; i++)
            {
                string part = pathParts[i];
                currentNode = currentNode[part] ?? throw new ArgumentException($"Path '{path}' not found in JSON.");
            }

            // Remove the last part
            string lastPart = pathParts[^1];
            if (currentNode is JsonObject jsonObject)
            {
                jsonObject.Remove(lastPart);
            }
            else
            {
                throw new InvalidOperationException("Cannot remove from a non-object node.");
            }
        }

        /// <summary>
        /// Gets the connection string from the JSON configuration
        /// </summary>
        /// <param name="jsonNode">Root JsonNode</param>
        /// <param name="connectionName">Name of the connection string (default: "DefaultConnection")</param>
        /// <returns>The connection string</returns>
        public static string GetConnectionString(JsonNode jsonNode, string connectionName = "DefaultConnection")
        {
            return GetValue<string>(jsonNode, $"ConnectionStrings.{connectionName}");
        }

        /// <summary>
        /// Sets the connection string in the JSON configuration
        /// </summary>
        /// <param name="jsonNode">Root JsonNode</param>
        /// <param name="connectionString">Connection string value</param>
        /// <param name="connectionName">Name of the connection string (default: "DefaultConnection")</param>
        public static void SetConnectionString(JsonNode jsonNode, string connectionString, string connectionName = "DefaultConnection")
        {
            SetValue(jsonNode, $"ConnectionStrings.{connectionName}", connectionString);
        }

        /// <summary>
        /// Gets an application setting from the JSON configuration
        /// </summary>
        /// <param name="jsonNode">Root JsonNode</param>
        /// <param name="settingName">Name of the setting</param>
        /// <returns>The setting value</returns>
        public static string GetApplicationSetting(JsonNode jsonNode, string settingName)
        {
            return GetValue<string>(jsonNode, $"ApplicationSettings.{settingName}");
        }

        /// <summary>
        /// Sets an application setting in the JSON configuration
        /// </summary>
        /// <param name="jsonNode">Root JsonNode</param>
        /// <param name="settingName">Name of the setting</param>
        /// <param name="value">Value to set</param>
        public static void SetApplicationSetting(JsonNode jsonNode, string settingName, string value)
        {
            SetValue(jsonNode, $"ApplicationSettings.{settingName}", value);
        }

        /// <summary>
        /// Serializes an object to a JSON string
        /// </summary>
        /// <param name="obj">The object to serialize</param>
        /// <returns>JSON string representation of the object</returns>
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
                // Log or handle the exception as needed
                throw new JsonSerializationException("Failed to serialize object to JSON", ex);
            }
        }

        /// <summary>
        /// Deserializes a JSON string to an object of the specified type
        /// </summary>
        /// <typeparam name="T">The type of object to deserialize to</typeparam>
        /// <param name="json">The JSON string to deserialize</param>
        /// <returns>The deserialized object</returns>
        public static T DeserializeObject<T>(string json)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(json))
                {
                    return default(T);
                }

                return JsonConvert.DeserializeObject<T>(json);
            }
            catch (Exception ex)
            {
                // Log or handle the exception as needed
                throw new JsonSerializationException($"Failed to deserialize JSON to type {typeof(T).Name}", ex);
            }
        }

    }
}