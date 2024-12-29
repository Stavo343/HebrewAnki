using HebrewAnki.Console.Enums;
using HebrewAnki.Data.Models;
using System.Text.Json;

namespace HebrewAnki.Console
{
    public class DeckBuilder
    {
        private Action<string> _log;
        
        private readonly List<LexicalIndexEntry> _lexicalIndexEntries;
        private readonly List<WlcBook> _wlcBooks;
        private readonly List<BdbEntry> _bdbEntries;
        private readonly List<OshmEntry> _oshmEntries;
        private readonly string _globalDeckNamePrefix;

        private readonly string _totalWordOccurrencesJsonPath = "../HebrewAnki.Data/json metadata/totalWordOccurrences.json";
        private Dictionary<string, int> _totalWordOccurrences = new();
        private readonly bool _totalWordOccurrencesNeedsUpdated = false;

        public DeckBuilder(
            Action<string> logFunction,
            DeckBuilderOptions deckBuilderOptions)
        {
            _log = logFunction;
            _lexicalIndexEntries = deckBuilderOptions.LexicalIndexEntries;
            _wlcBooks = deckBuilderOptions.WlcBooks;
            _bdbEntries = deckBuilderOptions.BdbEntries;
            _oshmEntries = deckBuilderOptions.OshmEntries;
            _globalDeckNamePrefix = deckBuilderOptions.GlobalDeckNamePrefix;

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

        public List<Deck> Build(List<Chapter> chaptersToBuild = null, List<Chapter> chaptersAlreadyBuilt = null, DeckScope deckScope = DeckScope.Chapter)
        {
            chaptersToBuild = chaptersToBuild ?? BookData.GetAllChapters();
            chaptersAlreadyBuilt = chaptersAlreadyBuilt ?? new();
            
            var lemmasToSkip = GetLemmasToSkip(chaptersAlreadyBuilt);
            var booksToBuild = chaptersToBuild.Select(c => BookData.WlcBookHebrewNames.First(x => x.Value == c.Book).Key).Distinct().ToList();
            var decks = new List<Deck>();

            foreach (var wlcBookName in booksToBuild)
            {
                var wlcBook = _wlcBooks.First(w => w.OsisId == wlcBookName);
                var hebrewBookName = BookData.WlcBookHebrewNames[wlcBookName];
                _log($"Parsing vocabulary for chapters from {hebrewBookName}");
                var bookDeckNamePrefix = $"{_globalDeckNamePrefix}::{hebrewBookName}";
                var chapterIndex = 0;

                var bookDeck = new Deck
                {
                    Name = $"{bookDeckNamePrefix}",
                };
                
                var bookChaptersToBuild = chaptersToBuild.Where(c => c.Book == hebrewBookName).Select(c => c.ChapterNumber).ToList();

                foreach (var chapter in wlcBook.Chapters)
                {
                    chapterIndex++;
                    if (!bookChaptersToBuild.Contains(chapterIndex))
                        continue;

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
                                && GetFormattedLemma(wlcWord.Lemma) != chainedLemma)
                                throw new InvalidDataException($"{hebrewBookName} {chapter}: {GetFormattedLemma(wlcWord.Lemma)} follows {chainedLemma} and does not match.");

                            chainedLemma = GetFormattedLemma(wlcWord.Lemma);

                            continue;
                        }

                        chainedLemma = null;
                        if (lemmasToSkip.Contains(GetFormattedLemma(wlcWord.Lemma)))
                            continue;

                        string word = null;
                        string definition = null;

                        var definitionIndex = 1;
                        var definitionList = new List<string>();
                        var lexicalIndexEntries = GetLexicalIndexEntries(wlcWord.Lemma);
                        word = lexicalIndexEntries.First().Word;

                        foreach (var lexicalEntry in lexicalIndexEntries)
                        {
                            var bdbEntry = _bdbEntries.First(b => b.Id == lexicalEntry.BdbIndex);

                            definitionList.Add($"{definitionIndex}. {bdbEntry.Definitions}");
                            definitionIndex++;
                        }
                        definition = string.Join(" <br /> ", definitionList);

                        if (_totalWordOccurrencesNeedsUpdated)
                        {
                            if (_totalWordOccurrences.ContainsKey(word))
                                _totalWordOccurrences[word]++;
                            else
                                _totalWordOccurrences.Add(word, 1);
                        }

                        var oshmEntry = _oshmEntries.First(e => e.MorphologyCode == wlcWord.Morph);

                        var variation = wlcWord.Value.Replace("/", "");

                        var existingNote = deck.Notes.FirstOrDefault(n => n.Word == word)
                            ?? decks.SelectMany(d => d.Notes).FirstOrDefault(n => n.Word == word);

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
                        else if (existingNote.Variations.All(v => v.Variation != variation))
                            existingNote.Variations.Add(
                                new()
                                {
                                    Variation = variation,
                                    Oshm = oshmEntry.Value
                                });
                    }

                    if (deckScope == DeckScope.Chapter)
                        decks.Add(deck);
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

        private List<string> GetLemmasToSkip(List<Chapter> chaptersAlreadyBuilt)
        {
            var result = new List<string>();
            var hebrewBookNamesToCheck = chaptersAlreadyBuilt.Select(c => c.Book).Distinct().ToList();
            
            foreach (var wlcBook in _wlcBooks)
            {
                var hebrewBookName = BookData.WlcBookHebrewNames[wlcBook.OsisId];
                if (!hebrewBookNamesToCheck.Contains(hebrewBookName))
                    continue;
                
                var chaptersToCheck = chaptersAlreadyBuilt
                    .Where(c => c.Book == hebrewBookName)
                    .Select(c => c.ChapterNumber).ToList();
                
                var chapterNumber = 0;
                foreach (var wlcChapter in wlcBook.Chapters)
                {
                    chapterNumber++;
                    
                    if (!chaptersToCheck.Contains(chapterNumber))
                        continue;
                    
                    result.AddRange(
                        wlcChapter.Verses.SelectMany(v => v.Words)
                        .SelectMany(w => GetLexicalIndexEntries(w.Lemma).Select(l => l.StrongsIndex))
                        .Distinct());
                }
            }

            return result;
        }

        private string GetFormattedLemma(string lemma)
        {
            lemma = lemma.Replace("+", "");
            
            while (lemma.Contains("/"))
                lemma = lemma.Substring(2);

            if (lemma.Contains(" "))
                lemma = lemma.Substring(0, lemma.Length - 2);

            return lemma;
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

            var result = _lexicalIndexEntries.Where(e => e.StrongsIndex == strong)
                .OrderBy(e => e.Aug)
                .ToList();
            
            result.AddRange(_lexicalIndexEntries.Where(e =>
                e.Word == result.First().Word
                && e.StrongsIndex != strong));

            return result;
        }
    }
}
