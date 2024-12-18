using System.Xml;
using HebrewAnki.Data.Models;

namespace HebrewAnki.Data.XmlParsers
{
    public static class BdbParser
    {
        public static List<BdbEntry> ParseBdb(string xmlPath)
        {
            var entries = new List<BdbEntry>();

            var bdbXml = new XmlDocument();
            bdbXml.Load(xmlPath);
            var parts = bdbXml.LastChild! as XmlElement;

            foreach (XmlElement part in parts.ChildNodes)
                foreach (XmlElement section in part.ChildNodes)
                    entries.AddRange(ExtractEntriesFromSection(section));

            return entries;
        }

        private static List<BdbEntry> ExtractEntriesFromSection(XmlElement section)
        {
            var entries = new List<BdbEntry>();

            foreach (XmlElement entry in section.ChildNodes)
            {
                if (entry.Name != "entry")
                    continue;

                var entryHtml = string.Empty;

                foreach (XmlNode child in entry.ChildNodes)
                    entryHtml += ExtractHtmlFromEntryChild(child);

                entries.Add(new()
                {
                    Id = entry.Attributes!["id"]?.Value?.ToString()!,
                    Definitions = entryHtml
                });

                //var w = entry.FirstChild!;

                //var def = w.NextSibling!;
                //while (def != null && def.Name != "def")
                //    def = def.NextSibling!;
                //if (def == null)
                //    continue;

                //var xref = def.NextSibling!;

                //entries.Add(new()
                //{
                //    Word = w.InnerText,
                //    Definition = def.InnerText,
                //    StrongsIndex = xref.Attributes!["strong"]?.Value?.ToString()!,
                //    Aug = xref.Attributes!["aug"]?.Value?.ToString()
                //});
            }

            return entries;
        }

        private static string ExtractHtmlFromEntryChild(XmlNode child, int indentCount = 0)
        {
            switch (child.NodeType)
            {
                case XmlNodeType.Element:
                    switch (child.Name)
                    {
                        case "sense":
                            {
                                indentCount++;
                                var senseId = child.Attributes!["n"]?.Value?.ToString()!;
                                var style = $" style=\"margin-left:{indentCount * 10}px; text-align:left;\"";

                                var senseAsHtml = $"<div{style}>{senseId}. ";

                                foreach (XmlNode senseChild in child.ChildNodes)
                                    senseAsHtml += ExtractHtmlFromEntryChild(senseChild, indentCount);

                                return $"{senseAsHtml}</div>";
                            }
                        case "pos":
                            return $"<span class=\"pos\">{child.InnerText}</span>";
                        case "def":
                            return $"<span class=\"def\">{child.InnerText}</span>";
                        case "em":
                            return $"<em>{child.InnerText}</em>";
                        case "stem":
                            return $"<span class=\"stem\">{child.InnerText}</span>";
                        case "asp":
                            return $"<span class=\"asp\">{child.InnerText}</span>";
                        case "foreign":
                            return $"<span class=\"foreign\">{child.InnerText}</span>";
                        case "w":
                        case "status":
                        case "ref":
                        case "page":
                            return string.Empty;
                        default:
                            throw new NotImplementedException($"XmlElement name {child.Name} not implemented.");
                    }
                case XmlNodeType.Text:
                    return child.InnerText;
                case XmlNodeType.Comment:
                    return string.Empty;
                default:
                    throw new NotImplementedException($"XmlNode type {child.NodeType} not implemented.");
            }
        }
    }
}
