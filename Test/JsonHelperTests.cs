using System.Text.Json.Nodes;
using Utils.Json;

namespace Utils.Test;

public class JsonHelperTests
{
    private sealed class SampleModel
    {
        public string Name { get; set; } = string.Empty;

        public int Value { get; set; }
    }

    [Fact]
    public void InitializeDefaultConfig_ContainsExpectedSections()
    {
        var config = JsonHelper.InitializeDefaultConfig();

        Assert.True(config.ContainsKey("ConnectionStrings"));
        Assert.True(config.ContainsKey("ApplicationSettings"));
        Assert.True(config.ContainsKey("Logging"));
        Assert.Equal("*", config["AllowedHosts"]!.GetValue<string>());
    }

    [Fact]
    public void CreateNewJsonObject_ReturnsEmptyObject()
    {
        var result = JsonHelper.CreateNewJsonObject();

        Assert.Empty(result);
    }

    [Fact]
    public void GetValue_ReturnsNestedValue()
    {
        var node = JsonHelper.InitializeDefaultConfig();

        var result = JsonHelper.GetValue<string>(node, "ApplicationSettings.Version");

        Assert.Equal("1.0.0", result);
    }

    [Fact]
    public void GetValue_WhenPathMissing_Throws()
    {
        var node = JsonHelper.InitializeDefaultConfig();

        Assert.Throws<ArgumentException>(
            () => JsonHelper.GetValue<string>(node, "Missing.Value"));
    }

    [Fact]
    public void SetValue_CreatesMissingPathAndSetsValue()
    {
        var node = JsonHelper.CreateNewJsonObject();

        JsonHelper.SetValue(node, "ApplicationSettings.AppName", "Fatora");

        Assert.Equal("Fatora", JsonHelper.GetValue<string>(node, "ApplicationSettings.AppName"));
    }

    [Fact]
    public void AddSection_AddsObject()
    {
        var node = JsonHelper.CreateNewJsonObject();
        var section = new JsonObject { ["key"] = "value" };

        JsonHelper.AddSection(node, "Custom", section);

        Assert.Same(section, node["Custom"]);
    }

    [Fact]
    public void AddSection_WhenExists_Throws()
    {
        var node = JsonHelper.InitializeDefaultConfig();

        Assert.Throws<ArgumentException>(
            () => JsonHelper.AddSection(node, "Logging", new JsonObject()));
    }

    [Fact]
    public void AddSection_WhenContentNull_Throws()
    {
        var node = JsonHelper.CreateNewJsonObject();

        Assert.Throws<ArgumentNullException>(
            () => JsonHelper.AddSection(node, "Custom", null!));
    }

    [Fact]
    public void RemoveSection_RemovesNestedSection()
    {
        var node = JsonHelper.InitializeDefaultConfig();

        JsonHelper.RemoveSection(node, "ApplicationSettings.Environment");

        Assert.False(((JsonObject)node["ApplicationSettings"]!).ContainsKey("Environment"));
    }

    [Fact]
    public void RemoveSection_WhenMissing_Throws()
    {
        var node = JsonHelper.CreateNewJsonObject();

        Assert.Throws<ArgumentException>(
            () => JsonHelper.RemoveSection(node, "Missing"));
    }

    [Fact]
    public void ConnectionStringHelpers_GetAndSet()
    {
        var node = JsonHelper.InitializeDefaultConfig();

        JsonHelper.SetConnectionString(node, "Server=.;Database=Test;");
        var result = JsonHelper.GetConnectionString(node);

        Assert.Equal("Server=.;Database=Test;", result);
    }

    [Fact]
    public void ApplicationSettingHelpers_GetAndSet()
    {
        var node = JsonHelper.InitializeDefaultConfig();

        JsonHelper.SetApplicationSetting(node, "AppName", "Fatora");
        var result = JsonHelper.GetApplicationSetting(node, "AppName");

        Assert.Equal("Fatora", result);
    }

    [Fact]
    public void SerializeObject_ThenDeserializeObject_RoundTrips()
    {
        var model = new SampleModel { Name = "Test", Value = 5 };

        var json = JsonHelper.SerializeObject(model);
        var result = JsonHelper.DeserializeObject<SampleModel>(json);

        Assert.NotNull(result);
        Assert.Equal("Test", result!.Name);
        Assert.Equal(5, result.Value);
    }

    [Fact]
    public void SerializeObject_WhenNull_ReturnsEmpty()
    {
        Assert.Equal(string.Empty, JsonHelper.SerializeObject(null!));
    }

    [Fact]
    public void DeserializeObject_WhenEmpty_ReturnsDefault()
    {
        Assert.Null(JsonHelper.DeserializeObject<SampleModel>(""));
        Assert.Null(JsonHelper.DeserializeObject<SampleModel>("   "));
    }

    [Fact]
    public void DeepClone_ReturnsIndependentCopy()
    {
        var node = JsonHelper.InitializeDefaultConfig();

        var clone = JsonHelper.DeepClone(node);
        clone["ApplicationSettings"]!["Version"] = "2.0.0";

        Assert.Equal("1.0.0", node["ApplicationSettings"]!["Version"]!.GetValue<string>());
        Assert.Equal("2.0.0", clone["ApplicationSettings"]!["Version"]!.GetValue<string>());
    }

    [Fact]
    public void DeepClone_WhenNull_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => JsonHelper.DeepClone(null!));
    }

    [Fact]
    public void WriteJsonFile_ThenReadJsonFile_RoundTrips()
    {
        var path = Path.Combine(Path.GetTempPath(), $"fatora-json-{Guid.NewGuid():N}.json");
        var node = JsonHelper.InitializeDefaultConfig();

        try
        {
            JsonHelper.WriteJsonFile(path, node);
            var result = JsonHelper.ReadJsonFile(path);

            Assert.Equal(
                JsonHelper.GetValue<string>(node, "ApplicationSettings.Version"),
                JsonHelper.GetValue<string>(result, "ApplicationSettings.Version"));
        }
        finally
        {
            if (File.Exists(path))
                File.Delete(path);
        }
    }

    [Fact]
    public void ReadJsonFile_WhenMissing_Throws()
    {
        var path = Path.Combine(Path.GetTempPath(), $"fatora-missing-{Guid.NewGuid():N}.json");

        Assert.Throws<FileNotFoundException>(() => JsonHelper.ReadJsonFile(path));
    }
}
