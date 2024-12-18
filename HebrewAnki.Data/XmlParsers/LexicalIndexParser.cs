using System.Xml;
using HebrewAnki.Data.Models;

namespace HebrewAnki.Data.XmlParsers
{
    public static class LexicalIndexParser
    {
        public static List<LexicalIndexEntry> ParseLexicalIndex(string xmlPath)
        {
            var lexicalIndexEntries = new List<LexicalIndexEntry>();

            var hebrewLexiconXml = new XmlDocument();
            hebrewLexiconXml.Load(xmlPath);
            var hebrewPart = hebrewLexiconXml.LastChild!.FirstChild! as XmlElement;
            ExtractEntries(hebrewPart, lexicalIndexEntries);
            var aramaicPart = hebrewPart.NextSibling! as XmlElement;
            ExtractEntries(aramaicPart, lexicalIndexEntries);

            return lexicalIndexEntries;
        }

        private static List<LexicalIndexEntry> ExtractEntries(XmlElement part, List<LexicalIndexEntry> entries)
        {
            foreach (XmlElement entry in part.ChildNodes)
            {
                var w = entry.FirstChild!;

                var def = w.NextSibling!;
                while (def != null && def.Name != "def")
                    def = def.NextSibling!;
                if (def == null)
                    continue;

                var xref = def.NextSibling!;

                entries.Add(new()
                {
                    Word = w.InnerText,
                    Definition = def.InnerText,
                    BdbIndex = xref.Attributes!["bdb"]?.Value?.ToString()!,
                    StrongsIndex = xref.Attributes!["strong"]?.Value?.ToString()!,
                    Aug = xref.Attributes!["aug"]?.Value?.ToString()
                });
            }

            return entries;
        }
    }
}
