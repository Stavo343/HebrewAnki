using HebrewAnki.Data.Models;
using System.Xml;

namespace HebrewAnki.Data.XmlParsers
{
    public static class OshmParser
    {
        private static string _xmlPath = "../../../../HebrewAnki.Data/lexicon/Oshm.xml";

        public static List<OshmEntry> GetOshmEntries()
        {
            var oshmEntries = new List<OshmEntry>();

            var oshmXml = new XmlDocument();
            oshmXml.Load(_xmlPath);
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
