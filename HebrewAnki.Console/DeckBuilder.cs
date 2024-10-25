using HebrewAnki.Data.Models;

namespace HebrewAnki.Console
{
    public class DeckBuilder
    {
        private readonly List<LexicalIndexEntry> _lexicalIndexEntries;
        private readonly List<WlcBook> _wlcBooks;
        private readonly List<OshmEntry> _oshmEntries;

        private Dictionary<string, int> _masterWordList = new();
        private Dictionary<string, int> _masterRootList = new();

        public DeckBuilder(
            List<LexicalIndexEntry> lexicalIndexEntries,
            List<WlcBook> wlcBooks,
            List<OshmEntry> oshmEntries)
        {
            _lexicalIndexEntries = lexicalIndexEntries;
            _wlcBooks = wlcBooks;
            _oshmEntries = oshmEntries;
        }

        public List<Deck> Build()
        {
            var globalDeckNamePrefix = "CompleteTest";
            var decks = new List<Deck>();

            foreach (var wlcBookName in BookNames.WlcBookHebrewNames.Keys)
            {
                var wlcBook = _wlcBooks.First(w => w.OsisId == wlcBookName);
                var bookName = BookNames.WlcBookHebrewNames[wlcBookName];
                var bookDeckNamePrefix = $"{globalDeckNamePrefix}::{bookName}";
                var chapterIndex = 0;

                foreach (var chapter in wlcBook.Chapters)
                {
                    chapterIndex++;

                    var deck = new Deck
                    {
                        Name = $"{bookDeckNamePrefix}::Chapter {chapterIndex}",
                    };

                    string chainedLemma = null;

                    foreach (var wlcWord in chapter.Verses.SelectMany(v => v.Words))
                    {
                        if (wlcWord.Lemma.Contains("+"))
                        {
                            if (chainedLemma != null
                                && GetStrippedLemma(wlcWord.Lemma) != chainedLemma)
                                _ = 1;

                            chainedLemma = GetStrippedLemma(wlcWord.Lemma);

                            continue;
                        }

                        //if (wlcWord.Lemma == "d/7451")
                        //    _ = 1;

                        LexicalIndexEntry lexicalIndexEntry = null;

                        try
                        {
                            lexicalIndexEntry = GetLexicalIndexEntry(wlcWord.Lemma);
                        }
                        catch
                        {
                            if (wlcWord.Lemma != "d/7451"
                                && wlcWord.Lemma != "6887"
                                && wlcWord.Lemma != "3651")
                                _ = 1;
                            continue;
                        }

                        var word = chainedLemma == null
                            ? wlcWord.Value.Replace("/", "")
                            : lexicalIndexEntry!.Word;

                        if (_masterWordList.ContainsKey(word))
                            _masterWordList[word]++;
                        else
                            _masterWordList.Add(word, 1);

                        if (_masterRootList.ContainsKey(lexicalIndexEntry.Word))
                            _masterRootList[lexicalIndexEntry.Word]++;
                        else
                            _masterRootList.Add(lexicalIndexEntry.Word, 1);

                        var oshmEntry = _oshmEntries.First(e => e.MorphologyCode == wlcWord.Morph);

                        if (!deck.Notes.Any(n => n.PrintedText == word))
                            deck.Notes.Add(new Note
                            {
                                PrintedText = word,
                                Root = lexicalIndexEntry.Word,
                                Definition = lexicalIndexEntry.Definition,
                                Oshm = oshmEntry.Value
                                // have to wait to get occurrence counts
                            });

                        chainedLemma = null;
                    }

                    decks.Add(deck);
                }
            }

            foreach (var note in decks.SelectMany(d => d.Notes))
            {
                note.TotalOccurrences = _masterWordList[note.PrintedText];
                note.TotalRootOccurrences = _masterRootList[note.Root];
            }

            var sortedByTotalOccurrences = decks.SelectMany(d => d.Notes).OrderByDescending(d => d.TotalOccurrences).ToList();
            var sortedByTotalRootOccurrences = decks.SelectMany(d => d.Notes).OrderByDescending(d => d.TotalRootOccurrences).ToList();
            var hopefullyMatchesBHS = sortedByTotalOccurrences
                .Where(x => x.IsHebrew)
                .Select(x =>
                new
                {
                    Word = x.PrintedText,
                    Count = x.TotalOccurrences,
                })
                .Distinct()
                .Where(x => x.Count > 69).ToList();

            var chapters = decks.Select(x =>
            new
            {
                Chapter = x.Name,
                UniqueWords = x.Notes.Count,
            }).OrderByDescending(x => x.UniqueWords);

            return decks;
        }

        private LexicalIndexEntry GetLexicalIndexEntry(string lemma)
        {
            var strong = "";
            string aug = null;

            while (lemma.Contains("/"))
                lemma = lemma.Substring(2);

            if (lemma.Contains(" "))
            {
                aug = lemma[lemma.Length - 1].ToString();
                strong = lemma.Substring(0, lemma.Length - 2);
            }
            else
                strong = lemma;

            return _lexicalIndexEntries.First(e => e.StrongsIndex == strong && e.Aug == aug);
        }

        private string GetStrippedLemma(string lemma)
        {
            var result = new string(lemma);

            while (result.Contains("/"))
                result = result.Substring(2);

            result = result.Replace("+", "");

            return result;
        }
    }
}
