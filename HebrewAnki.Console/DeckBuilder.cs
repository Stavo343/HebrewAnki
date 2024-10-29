using HebrewAnki.Data.Models;

namespace HebrewAnki.Console
{
    public class DeckBuilder
    {
        private readonly List<LexicalIndexEntry> _lexicalIndexEntries;
        private readonly List<WlcBook> _wlcBooks;
        private readonly List<OshmEntry> _oshmEntries;

        private Dictionary<string, int> _masterWordList = new();

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
            var globalDeckNamePrefix = "Hebrew/Aramaic Vocab Per Chapter";
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
                            chainedLemma = null;
                            continue;
                        }

                        if (_masterWordList.ContainsKey(lexicalIndexEntry.Word))
                            _masterWordList[lexicalIndexEntry.Word]++;
                        else
                            _masterWordList.Add(lexicalIndexEntry.Word, 1);

                        var oshmEntry = _oshmEntries.First(e => e.MorphologyCode == wlcWord.Morph);

                        var existingNote = deck.Notes.FirstOrDefault(n => n.Word == lexicalIndexEntry.Word);

                        var variation = wlcWord.Value.Replace("/", "");

                        if (existingNote == null)
                            deck.Notes.Add(new Note
                            {
                                Word = lexicalIndexEntry.Word,
                                Definition = lexicalIndexEntry.Definition,
                                Variations =
                                [
                                    new()
                                    {
                                        Variation = variation,
                                        Oshm = oshmEntry.Value
                                    }
                                ]
                                // have to wait to get occurrence counts
                            });
                        else if (!existingNote.Variations.Any(v => v.Variation == variation))
                            existingNote.Variations.Add(
                                new()
                                {
                                    Variation = variation,
                                    Oshm = oshmEntry.Value
                                });

                        chainedLemma = null;
                    }

                    decks.Add(deck);

                    chainedLemma = null;
                }
            }

            foreach (var note in decks.SelectMany(d => d.Notes))
                note.TotalOccurrences = _masterWordList[note.Word];

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
