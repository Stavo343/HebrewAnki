using System.Text;
using System.Xml;
using HebrewAnki.Data.Models;

namespace HebrewAnki.Data.XmlParsers
{
    public static class LexicalIndexParser
    {
        public static List<LexicalIndexEntry> ParseLexicalIndex()
        {
            var lexicalIndexPath = $"{AppContext.BaseDirectory}/Data/lexicon/LexicalIndex.xml";
            if (!File.Exists(lexicalIndexPath))
                throw new FileNotFoundException($"LexicalIndex.xml not found at {lexicalIndexPath}");

            var lexicalIndexEntries = new List<LexicalIndexEntry>();
            var hebrewLexiconXml = new XmlDocument();
            hebrewLexiconXml.Load(lexicalIndexPath);
            var hebrewPart = hebrewLexiconXml.LastChild!.FirstChild! as XmlElement;
            ExtractEntries(hebrewPart, lexicalIndexEntries, "heb");
            var aramaicPart = hebrewPart.NextSibling! as XmlElement;
            ExtractEntries(aramaicPart, lexicalIndexEntries, "arc");

            return lexicalIndexEntries;
        }

        private static void ExtractEntries(XmlElement part, List<LexicalIndexEntry> entries, string languageCode)
        {
            foreach (XmlElement entry in part.ChildNodes)
            {
                string pos = null;
                var w = entry.FirstChild!;

                var currentSibling = w.NextSibling!;
                while (currentSibling != null && currentSibling.Name != "def")
                {
                    if (currentSibling.Name == "pos")
                        pos = currentSibling.InnerText;
                    
                    currentSibling = currentSibling.NextSibling!;
                }
                if (currentSibling == null)
                    continue;

                var xref = currentSibling.NextSibling!;

                entries.Add(new()
                {
                    Word = HebrewStringHelper.CleanAndNormalize(w.InnerText),
                    Pos = pos,
                    Definition = currentSibling.InnerText,
                    BdbIndex = xref.Attributes!["bdb"]?.Value?.ToString()!,
                    StrongsIndex = xref.Attributes!["strong"]?.Value?.ToString()!,
                    Aug = xref.Attributes!["aug"]?.Value?.ToString(),
                    LanguageCode = languageCode
                });
            }
        }
    }
}