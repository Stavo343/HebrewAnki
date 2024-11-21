using System.Xml;
using HebrewAnki.Data.Models;

namespace HebrewAnki.Data.XmlParsers
{
    public static class BdbParser
    {
        private static string _xmlPath = "../../../../HebrewAnki.Data/lexicon/bdb.xml";

        public static List<BdbEntry> ParseBdb()
        {
            var entries = new List<BdbEntry>();

            var bdbXml = new XmlDocument();
            bdbXml.Load(_xmlPath);
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
                    switch (child.NodeType)
                    {
                        case XmlNodeType.Element:
                            switch (child.Name)
                            {
                                case "w":
                                    entryHtml += $"<span class=\"Hebrew\">{child.InnerText}</span>";
                                    break;
                                case "pos":
                                    entryHtml += $"<span class=\"pos\">{child.InnerText}</span>";
                                    break;
                                case "def":
                                    entryHtml += $"<span class=\"def\">{child.InnerText}</span>";
                                    break;
                                case "em":
                                    entryHtml += $"<em>{child.InnerText}</em>";
                                    break;
                                case "stem":
                                    entryHtml += $"<span class=\"stem\">{child.InnerText}</span>";
                                    break;
                                case "asp":
                                    entryHtml += $"<span class=\"asp\">{child.InnerText}</span>";
                                    break;
                                case "foreign":
                                    entryHtml += $"<span class=\"foreign\">{child.InnerText}</span>";
                                    break;

                                case "sense":
                                case "status":
                                case "ref":
                                case "page":
                                    break;
                                default:
                                    throw new NotImplementedException($"XmlElement name {child.Name} not implemented.");
                            }
                            break;
                        case XmlNodeType.Text:
                            entryHtml += child.InnerText;
                            break;
                        case XmlNodeType.Comment:
                            break;
                        default:
                            throw new NotImplementedException($"XmlNode type {child.NodeType} not implemented.");
                    }

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
    }
}
