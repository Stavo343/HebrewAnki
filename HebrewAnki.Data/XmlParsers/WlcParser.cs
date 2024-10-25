using System.Xml;
using HebrewAnki.Data.Models;

namespace HebrewAnki.Data.XmlParsers
{
    public static class WlcParser
    {
        public static string _xmlDirectory = "../../../../HebrewAnki.Data/wlc/";

        public static List<WlcBook> GetWlcBooks()
        {
            var wlcBooks = new List<WlcBook>();

            foreach (var bookXmlFilePath in Directory.GetFiles(_xmlDirectory))
            {
                var bookXml = new XmlDocument();
                bookXml.Load(bookXmlFilePath);
                var bookElement = bookXml.FirstChild!.NextSibling!.FirstChild!.FirstChild!.NextSibling as XmlElement;

                var wlcBook = new WlcBook
                {
                    OsisId = bookElement!.GetAttribute("osisID")
                };

                var currentChapter = bookElement.FirstChild as XmlElement;
                while (currentChapter != null)
                {
                    var wlcChapter = new WlcChapter
                    {
                        OsisId = currentChapter!.GetAttribute("osisID")
                    };

                    var currentVerse = currentChapter.FirstChild as XmlElement;

                    while (currentVerse != null)
                    {
                        var wlcVerse = new WlcVerse
                        {
                            OsisId = currentVerse!.GetAttribute("osisID")
                        };

                        var currentWord = currentVerse.FirstChild as XmlElement;

                        while (currentWord != null)
                        {
                            if (currentWord.Name != "w")
                            {
                                currentWord = currentWord.NextSibling as XmlElement;
                                continue;
                            }

                            wlcVerse.Words.Add(new WlcWord
                            {
                                Value = currentWord.InnerText,
                                Lemma = currentWord.GetAttribute("lemma"),
                                Morph = currentWord.GetAttribute("morph")
                            });

                            currentWord = currentWord.NextSibling as XmlElement;
                        }

                        wlcChapter.Verses.Add(wlcVerse);

                        currentVerse = currentVerse.NextSibling as XmlElement;
                    }

                    wlcBook.Chapters.Add(wlcChapter);

                    currentChapter = currentChapter.NextSibling as XmlElement;
                }

                wlcBooks.Add(wlcBook);
            }

            return wlcBooks;
        }
    }
}
