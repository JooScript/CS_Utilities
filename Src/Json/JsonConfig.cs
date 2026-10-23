using System.Text.Json;
using Utils.Validate;

namespace Utils.Json;

/// <summary>
/// Wraps the raw JSON stored inside <see cref="TbContentField.Configuration"/> and
/// offers typed, case-insensitive accessors so strategies do not parse it
/// themselves.
/// </summary>
public class JsonConfig
{
    private readonly IDictionary<string, JsonElement>? _values;

    public JsonConfig(string? json)
    {
        _values = new Dictionary<string, JsonElement>(StringComparer.OrdinalIgnoreCase);

        if (string.IsNullOrWhiteSpace(json))
        {
            _values = null;
            return;
        }

        try
        {
            using JsonDocument doc = JsonDocument.Parse(json);

            if (doc.RootElement.ValueKind == JsonValueKind.Object)
            {
                foreach (JsonProperty property in doc.RootElement.EnumerateObject())
                    _values[property.Name] = property.Value.Clone();
            }
        }
        catch (JsonException)
        {
            _values = null;
        }
    }

    #region Get Methods

    public int GetInt(string key, int defaultValue)
    {
        if (_values is null)
            return defaultValue;

        if (!_values.TryGetValue(key, out JsonElement element))
            return defaultValue;

        int value = 0;

        if (element.ValueKind == JsonValueKind.Number)
            if (element.TryGetInt32(out value))
                return value;
            else
                return defaultValue;

        if (int.TryParse(element.ToString(), out value))
            return value;
        else
            return defaultValue;
    }

    public long GetLong(string key, long defaultValue)
    {
        if (_values is null)
            return defaultValue;

        if (!_values.TryGetValue(key, out JsonElement element))
            return defaultValue;

        long value = 0;

        if (element.ValueKind == JsonValueKind.Number)
            if (element.TryGetInt64(out value))
                return value;
            else
                return defaultValue;

        if (long.TryParse(element.ToString(), out value))
            return value;
        else
            return defaultValue;
    }

    public decimal GetDecimal(string key, decimal defaultValue)
    {
        if (_values is null)
            return defaultValue;

        if (!_values.TryGetValue(key, out JsonElement element))
            return defaultValue;

        decimal value = 0;

        if (element.ValueKind == JsonValueKind.Number)
            if (element.TryGetDecimal(out value))
                return value;
            else
                return defaultValue;

        if (decimal.TryParse(element.ToString(), out value))
            return value;
        else
            return defaultValue;
    }

    public string? GetString(string key, string? defaultValue = null)
    {
        if (_values is null)
            return defaultValue;

        if (!_values.TryGetValue(key, out var value))
            return defaultValue;

        return value.ToString();
    }

    public bool GetBool(string key, bool defaultValue = false)
    {
        if (_values is null)
            return defaultValue;

        if (!_values.TryGetValue(key, out var value))
            return defaultValue;

        return bool.TryParse(value.ToString(), out var result)
            ? result
            : defaultValue;
    }

    public TEnum GetEnum<TEnum>(string key, TEnum defaultValue) where TEnum : struct, Enum
    {
        if (_values is null)
            return defaultValue;

        if (!_values.TryGetValue(key, out JsonElement value))
            return defaultValue;

        if (value.ValueKind == JsonValueKind.String &&
            Enum.TryParse<TEnum>(
                value.GetString(),
                true,
                out var stringResult))
        {
            return stringResult;
        }

        if (value.ValueKind == JsonValueKind.Number &&
            value.TryGetInt32(out int intValue) &&
            Enum.IsDefined(typeof(TEnum), intValue))
        {
            return (TEnum)Enum.ToObject(typeof(TEnum), intValue);
        }

        return defaultValue;
    }


    #endregion

    #region Try Get Methods

    public bool TryGetInt(string key, out int value)
    {
        value = 0;

        if (_values is null)
            return false;

        if (!_values.TryGetValue(key, out JsonElement element))
            return false;

        if (element.ValueKind == JsonValueKind.Number)
            if (element.TryGetInt32(out value))
                return true;
            else
                return false;

        if (int.TryParse(element.ToString(), out value))
            return true;
        else
            return false;
    }

    public bool TryGetString(string key, out string? value)
    {
        value = null;

        if (_values is null)
            return false;

        if (!_values.TryGetValue(key, out JsonElement element))
            return false;

        value = element.ValueKind switch
        {
            JsonValueKind.String => element.GetString(),
            _ => element.ToString(),
        };
        return true;
    }

    public bool TryGetBool(string key, out bool value)
    {
        value = false;

        if (_values is null)
            return false;

        if (!_values.TryGetValue(key, out JsonElement element))
            return false;

        if (element.ValueKind == JsonValueKind.True)
        {
            value = true;
            return true;
        }

        if (element.ValueKind == JsonValueKind.False)
        {
            value = false;
            return true;
        }

        return bool.TryParse(element.ToString(), out value);
    }

    public bool TryGetStringList(string key, out IReadOnlyList<string> value)
    {
        value = [];

        if (_values is null)
            return false;

        if (!_values.TryGetValue(key, out JsonElement element))
            return false;

        var result = new List<string>();
        switch (element.ValueKind)
        {
            case JsonValueKind.Array:
                foreach (JsonElement item in element.EnumerateArray())
                    result.Add(item.ToString());
                break;
            case JsonValueKind.String:
                result.Add(element.GetString()!);
                break;
            default:
                return false;
        }

        value = result;
        return true;
    }

    #endregion

    public bool IsEnum<TEnum>(string key)
    where TEnum : struct, Enum
    {
        if (_values is null)
            return false;

        if (!_values.TryGetValue(key, out JsonElement value))
            return false;

        return ValidationHelper.IsEnum<TEnum>(value);
    }

    public bool IsExist(string key)
        => (_values is not null) && _values.TryGetValue(key, out JsonElement _);

}