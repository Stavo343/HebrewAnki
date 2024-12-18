using HebrewAnki.Data.Models;
using System.Xml;

namespace HebrewAnki.Data.XmlParsers
{
    public static class OshmParser
    {
        public static List<OshmEntry> ParseOshmEntries(string xmlPath)
        {
            var oshmEntries = new List<OshmEntry>();

            var oshmXml = new XmlDocument();
            oshmXml.Load(xmlPath);
            var currentEntry = oshmXml.FirstChild!.NextSibling!.FirstChild!.NextSibling!.FirstChild!.FirstChild! as XmlElement;

            while (currentEntry != null)
            {
                var test = currentEntry.GetAttribute("n");

                oshmEntries.Add(new OshmEntry
                {
                    MorphologyCode = currentEntry.GetAttribute("n"),
                    Value = currentEntry.InnerText
                });

                currentEntry = currentEntry.NextSibling as XmlElement;
            }

            return oshmEntries;
        }
    }
}
