using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace Utils.Xml;

public static class XmlHelper
{
    public static XmlDocument ToXmlDoc(string xmlPath)
    {
        if (!File.Exists(xmlPath))
            throw new Exception("Invoice XML not found");

        var xml = new XmlDocument();
        xml.Load(xmlPath);

        return xml;
    }

    public static XmlDocument ToXmlDocFromBase64(string base64String)
    {
        var xmlBytes = Convert.FromBase64String(base64String);
        var xmlString = Encoding.UTF8.GetString(xmlBytes);

        var doc = new XmlDocument();
        doc.LoadXml(xmlString);
        return doc;
    }

    public static XmlDocument ToXmlDoc(XDocument xDoc)
    {
        if (xDoc == null)
            throw new ArgumentNullException(nameof(xDoc));

        var xmlDoc = new XmlDocument();

        using (var reader = xDoc.CreateReader())
        {
            xmlDoc.Load(reader);
        }

        return xmlDoc;
    }

    public static string ToXmlString(XDocument doc)
    {
        var settings = new XmlWriterSettings
        {
            Indent = true,
            IndentChars = "  ",
            Encoding = Encoding.UTF8
        };
        using var sw = new StringWriter();
        using var xw = XmlWriter.Create(sw, settings);
        doc.Save(xw);
        return sw.ToString();
    }

}
