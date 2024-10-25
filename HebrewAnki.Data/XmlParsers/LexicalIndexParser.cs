using System.Xml;
using HebrewAnki.Data.Models;

namespace HebrewAnki.Data.XmlParsers
{
    public static class LexicalIndexParser
    {
        private static string _xmlPath = "../../../../HebrewAnki.Data/lexicon/LexicalIndex.xml";

        public static List<LexicalIndexEntry> ParseLexicalIndex()
        {
            var lexicalIndexEntries = new List<LexicalIndexEntry>();

            var hebrewLexiconXml = new XmlDocument();
            hebrewLexiconXml.Load(_xmlPath);
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
                //if (entry.Attributes!["id"]?.Value?.ToString()! == "kio")
                //    _ = 1;

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
                    StrongsIndex = xref.Attributes!["strong"]?.Value?.ToString()!,
                    Aug = xref.Attributes!["aug"]?.Value?.ToString()
                });
            }

            return entries;
        }
    }
}
