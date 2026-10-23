using System.Text;
using System.Xml.Linq;
using Utils.Xml;

namespace Utils.Test;

public class XmlHelperTests
{
    [Fact]
    public void ToXmlDocFromString_LoadsXml()
    {
        var result = XmlHelper.ToXmlDocFromString("<root><child>value</child></root>");

        Assert.Equal("root", result.DocumentElement!.Name);
        Assert.Equal("value", result.DocumentElement.SelectSingleNode("child")!.InnerText);
    }

    [Fact]
    public void ToXmlDocFromBase64_LoadsXml()
    {
        var xml = "<root><child>value</child></root>";
        var base64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(xml));

        var result = XmlHelper.ToXmlDocFromBase64(base64);

        Assert.Equal("root", result.DocumentElement!.Name);
    }

    [Fact]
    public void ToXmlDoc_FromXDocument_LoadsXml()
    {
        var xDoc = XDocument.Parse("<root><child>value</child></root>");

        var result = XmlHelper.ToXmlDoc(xDoc);

        Assert.Equal("root", result.DocumentElement!.Name);
    }

    [Fact]
    public void ToXmlDoc_WhenNull_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => XmlHelper.ToXmlDoc(null!));
    }

    [Fact]
    public void ToXmlString_ProducesIndentedXml()
    {
        var doc = new XDocument(
            new XElement("root",
                new XElement("child", "value")));

        var result = XmlHelper.ToXmlString(doc);

        Assert.Contains("<root>", result);
        Assert.Contains("  <child>value</child>", result);
    }

    [Fact]
    public void ToXmlDocFromPath_WhenMissing_Throws()
    {
        var path = Path.Combine(Path.GetTempPath(), $"fatora-missing-{Guid.NewGuid():N}.xml");

        var exception = Assert.Throws<Exception>(() => XmlHelper.ToXmlDocFromPath(path));

        Assert.Equal("Invoice XML not found", exception.Message);
    }

    [Fact]
    public void ToXmlDocFromPath_WhenExists_LoadsXml()
    {
        var path = Path.Combine(Path.GetTempPath(), $"fatora-xml-{Guid.NewGuid():N}.xml");

        try
        {
            File.WriteAllText(path, "<root><child>value</child></root>");

            var result = XmlHelper.ToXmlDocFromPath(path);

            Assert.Equal("root", result.DocumentElement!.Name);
        }
        finally
        {
            if (File.Exists(path))
                File.Delete(path);
        }
    }
}
