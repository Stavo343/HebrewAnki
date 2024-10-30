using HebrewAnki.Console.Enums;
using HebrewAnki.Data.Models;
using System.Text.Json;

namespace HebrewAnki.Console
{
    public class DeckBuilder
    {
        private readonly List<LexicalIndexEntry> _lexicalIndexEntries;
        private readonly List<WlcBook> _wlcBooks;
        private readonly List<OshmEntry> _oshmEntries;

        private readonly string _totalWordOccurrencesJsonPath = "../../../../HebrewAnki.Data/json metadata/totalWordOccurrences.json";
        private Dictionary<string, int> _totalWordOccurrences = new();
        private readonly bool _totalWordOccurrencesNeedsUpdated = false;

        public DeckBuilder(
            List<LexicalIndexEntry> lexicalIndexEntries,
            List<WlcBook> wlcBooks,
            List<OshmEntry> oshmEntries)
        {
            _lexicalIndexEntries = lexicalIndexEntries;
            _wlcBooks = wlcBooks;
            _oshmEntries = oshmEntries;

            try
            {
                var totalWordOccurrencesJson = File.ReadAllText(_totalWordOccurrencesJsonPath);
                _totalWordOccurrences = JsonSerializer.Deserialize<Dictionary<string, int>>(totalWordOccurrencesJson)!;
            }
            catch
            {
                _totalWordOccurrencesNeedsUpdated = true;
            }
        }

        public List<Deck> Build(DeckScope deckScope)
        {
            var globalDeckNamePrefix = $"Hebrew/Aramaic Vocab Per {deckScope.ToString()}";
            var decks = new List<Deck>();

            foreach (var wlcBookName in BookNames.WlcBookHebrewNames.Keys)
            {
                var wlcBook = _wlcBooks.First(w => w.OsisId == wlcBookName);
                var bookName = BookNames.WlcBookHebrewNames[wlcBookName];
                var bookDeckNamePrefix = $"{globalDeckNamePrefix}::{bookName}";
                var chapterIndex = 0;

                var bookDeck = new Deck
                {
                    Name = $"{bookDeckNamePrefix}",
                };

                foreach (var chapter in wlcBook.Chapters)
                {
                    chapterIndex++;

                    var deck = deckScope == DeckScope.Book
                        ? bookDeck
                        : new Deck
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
                                throw new InvalidDataException($"{bookName} {chapter}: {GetStrippedLemma(wlcWord.Lemma)} follows {chainedLemma} and does not match.");

                            chainedLemma = GetStrippedLemma(wlcWord.Lemma);

                            continue;
                        }

                        string word = null;
                        string definition = null;

                        try
                        {
                            var lexicalIndexEntry = GetLexicalIndexEntry(wlcWord.Lemma);
                            word = lexicalIndexEntry.Word;
                            definition = lexicalIndexEntry.Definition;
                        }
                        catch
                        {
                            var definitionIndex = 1;
                            var definitionList = new List<string>();
                            var lexicalIndexEntries = GetLexicalIndexEntries(wlcWord.Lemma);
                            word = lexicalIndexEntries.First().Word;

                            foreach (var lexicalEntry in lexicalIndexEntries)
                            {
                                definitionList.Add($"{definitionIndex}. {lexicalEntry.Definition}");
                                definitionIndex++;
                            }
                            definition = string.Join(" <br /> ", definitionList);
                        }

                        if (_totalWordOccurrencesNeedsUpdated)
                        {
                            if (_totalWordOccurrences.ContainsKey(word))
                                _totalWordOccurrences[word]++;
                            else
                                _totalWordOccurrences.Add(word, 1);
                        }

                        var oshmEntry = _oshmEntries.First(e => e.MorphologyCode == wlcWord.Morph);

                        var existingNote = deck.Notes.FirstOrDefault(n => n.Word == word);

                        var variation = wlcWord.Value.Replace("/", "");

                        if (existingNote == null)
                            deck.Notes.Add(new Note
                            {
                                Word = word,
                                Definition = definition,
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

                    if (deckScope == DeckScope.Chapter)
                        decks.Add(deck);

                    chainedLemma = null;
                }

                if (deckScope == DeckScope.Book)
                    decks.Add(bookDeck);
            }

            foreach (var note in decks.SelectMany(d => d.Notes))
                note.TotalOccurrences = _totalWordOccurrences[note.Word];

            if (_totalWordOccurrencesNeedsUpdated)
            {
                var totalWordOccurrencesJson = JsonSerializer.Serialize(_totalWordOccurrences);
                File.Delete(_totalWordOccurrencesJsonPath);
                File.WriteAllText(_totalWordOccurrencesJsonPath, totalWordOccurrencesJson);
            }

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

        private List<LexicalIndexEntry> GetLexicalIndexEntries(string lemma)
        {
            while (lemma.Contains("/"))
                lemma = lemma.Substring(2);

            var strong = lemma.Contains(" ")
                ? lemma.Substring(0, lemma.Length - 2)
                : lemma;

            return _lexicalIndexEntries.Where(e => e.StrongsIndex == strong)
                .OrderBy(e => e.Aug)
                .ToList();
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
