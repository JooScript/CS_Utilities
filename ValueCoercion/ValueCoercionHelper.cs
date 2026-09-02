using System.Collections;
using System.Globalization;
using System.Text.Json;

namespace Utils.ValueCoercion;

public static class ValueCoercionHelper
{
    /// <summary>
    /// True when the value is null, a JSON null, an empty string, or an empty array.
    /// </summary>
    public static bool IsEmpty(object? value)
    {
        if (value is null)
            return true;

        if (value is JsonElement element)
        {
            return element.ValueKind switch
            {
                JsonValueKind.Null or JsonValueKind.Undefined => true,
                JsonValueKind.String => string.IsNullOrWhiteSpace(element.GetString()),
                JsonValueKind.Array => element.GetArrayLength() == 0,
                _ => false,
            };
        }

        return value switch
        {
            string s => string.IsNullOrWhiteSpace(s),
            IEnumerable e when e is not string => !e.Cast<object?>().Any(),
            _ => false,
        };
    }

    public static string? AsString(object? value)
    {
        if (value is null)
            return null;

        if (value is JsonElement element)
        {
            return element.ValueKind switch
            {
                JsonValueKind.String => element.GetString(),
                JsonValueKind.Null or JsonValueKind.Undefined => null,
                _ => element.ToString(),
            };
        }

        return Convert.ToString(value, CultureInfo.InvariantCulture);
    }

    public static bool AsBoolean(object? value, out bool result)
    {
        result = false;
        if (value is null)
            return false;

        if (value is bool b)
        {
            result = b;
            return true;
        }

        string? text = AsString(value);
        return bool.TryParse(text, out result) || int.TryParse(text, out int num) && (result = num != 0);
    }

    public static bool AsInteger(object? value, out int result)
    {
        result = 0;
        if (value is null)
            return false;

        if (value is int i)
        {
            result = i;
            return true;
        }

        if (value is long l && l is >= int.MinValue and <= int.MaxValue)
        {
            result = (int)l;
            return true;
        }

        if (value is JsonElement { ValueKind: JsonValueKind.Number } element && element.TryGetInt32(out int json))
        {
            result = json;
            return true;
        }

        return int.TryParse(AsString(value), NumberStyles.Integer, CultureInfo.InvariantCulture, out result);
    }

    public static bool AsDecimal(object? value, out decimal result)
    {
        result = 0m;
        if (value is null)
            return false;

        if (value is decimal d)
        {
            result = d;
            return true;
        }

        if (value is double db)
        {
            result = (decimal)db;
            return true;
        }

        if (value is int i)
        {
            result = i;
            return true;
        }

        if (value is JsonElement { ValueKind: JsonValueKind.Number } element && element.TryGetDecimal(out decimal json))
        {
            result = json;
            return true;
        }

        return decimal.TryParse(AsString(value), NumberStyles.Number, CultureInfo.InvariantCulture, out result);
    }

    public static bool AsDateOnly(object? value, out DateOnly result)
    {
        result = default;
        if (value is null)
            return false;

        if (value is DateOnly date)
        {
            result = date;
            return true;
        }

        if (value is DateTime dt)
        {
            result = DateOnly.FromDateTime(dt);
            return true;
        }

        return DateOnly.TryParse(AsString(value), CultureInfo.InvariantCulture, DateTimeStyles.None, out result);
    }

    public static bool AsDateTime(object? value, out DateTime result)
    {
        result = default;
        if (value is null)
            return false;

        if (value is DateTime dt)
        {
            result = dt;
            return true;
        }

        return DateTime.TryParse(AsString(value), CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out result);
    }

    public static bool AsTime(object? value, out TimeSpan result)
    {
        result = default;
        if (value is null)
            return false;

        if (value is TimeSpan ts)
        {
            result = ts;
            return true;
        }

        if (value is JsonElement { ValueKind: JsonValueKind.String } element)
            return TimeSpan.TryParse(element.GetString(), CultureInfo.InvariantCulture, out result);

        return TimeSpan.TryParse(AsString(value), CultureInfo.InvariantCulture, out result);
    }

    public static bool AsGuid(object? value, out Guid result)
    {
        result = default;
        if (value is null)
            return false;

        if (value is Guid g)
        {
            result = g;
            return true;
        }

        if (value is JsonElement { ValueKind: JsonValueKind.String } element)
            return Guid.TryParse(element.GetString(), out result);

        return Guid.TryParse(AsString(value), out result);
    }

    /// <summary>
    /// Extracts a flat list of scalar strings. Accepts string arrays,
    /// generic sequences of primitives/GUIDs, JSON arrays and a JSON string
    /// that is itself an array.
    /// </summary>
    public static List<string> AsStringList(object? value, out bool isList)
    {
        isList = true;
        var result = new List<string>();

        if (value is null)
        {
            isList = false;
            return result;
        }

        if (value is JsonElement { ValueKind: JsonValueKind.Array } arr)
        {
            foreach (JsonElement item in arr.EnumerateArray())
                result.Add(item.ToString());
            return result;
        }

        if (value is JsonElement { ValueKind: JsonValueKind.String } str)
        {
            string? text = str.GetString();
            if (TryParseJsonArray(text, result))
                return result;

            result.Add(text!);
            isList = false;
            return result;
        }

        if (value is string s)
        {
            if (TryParseJsonArray(s, result))
                return result;

            result.Add(s);
            isList = false;
            return result;
        }

        if (value is IEnumerable enumerable and not string)
        {
            foreach (object? item in enumerable)
            {
                result.Add(item switch
                {
                    null => string.Empty,
                    JsonElement e => e.ToString(),
                    _ => Convert.ToString(item, CultureInfo.InvariantCulture) ?? string.Empty,
                });
            }
            return result;
        }

        result.Add(Convert.ToString(value, CultureInfo.InvariantCulture) ?? string.Empty);
        isList = false;
        return result;
    }

    /// <summary>
    /// Converts a value to a nested dictionary (component values).
    /// </summary>
    public static bool AsDictionary(object? value, out Dictionary<string, object?> result)
    {
        result = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);

        if (value is null)
            return false;

        if (value is JsonElement { ValueKind: JsonValueKind.Object } obj)
        {
            foreach (JsonProperty property in obj.EnumerateObject())
                result[property.Name] = property.Value;
            return true;
        }

        if (value is IDictionary<string, object?> dict)
        {
            foreach (KeyValuePair<string, object?> pair in dict)
                result[pair.Key] = pair.Value;
            return true;
        }

        return false;
    }

    public static bool IsValidJsonString(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return false;

        try
        {
            using JsonDocument doc = JsonDocument.Parse(json);
            return true;
        }
        catch (JsonException)
        {
            return false;
        }
    }

    private static bool TryParseJsonArray(string? text, List<string> result)
    {
        if (string.IsNullOrWhiteSpace(text) || !text.TrimStart().StartsWith("["))
            return false;

        try
        {
            using JsonDocument doc = JsonDocument.Parse(text);
            if (doc.RootElement.ValueKind != JsonValueKind.Array)
                return false;

            foreach (JsonElement item in doc.RootElement.EnumerateArray())
                result.Add(item.ToString());
            return true;
        }
        catch (JsonException)
        {
            return false;
        }
    }

}
