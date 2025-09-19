using System.Text;
using HebrewAnki.Data.Models;
using System.Text.Json;
using HebrewAnki.Data;
using HebrewAnki.Enums;

namespace HebrewAnki
{
    public class DeckBuilder
    {
        private Action<string> _log;
        
        private readonly List<LexicalIndexEntry> _lexicalIndexEntries;
        private readonly List<WlcBook> _wlcBooks;
        private readonly List<BdbEntry> _bdbEntries;
        private readonly string _globalDeckNamePrefix;
        private readonly bool _ignoreProperNouns;
        private readonly bool _ignoreAramaic;
        private readonly int _minimumNumberOfOccurrences;
        private readonly int _maximumNumberOfOccurrences;

        private readonly string _totalWordOccurrencesJsonPath = "../HebrewAnki.Data/json metadata/totalWordOccurrences.json";
        private Dictionary<string, Dictionary<string, Dictionary<string, int>>> _totalWordOccurrences = new();
        private readonly bool _totalWordOccurrencesNeedsUpdated = false;

        public DeckBuilder(
            Action<string> logFunction,
            DeckBuilderOptions deckBuilderOptions)
        {
            _log = logFunction;
            _lexicalIndexEntries = deckBuilderOptions.LexicalIndexEntries;
            _wlcBooks = deckBuilderOptions.WlcBooks;
            _bdbEntries = deckBuilderOptions.BdbEntries;
            _globalDeckNamePrefix = deckBuilderOptions.GlobalDeckNamePrefix;
            _ignoreProperNouns = deckBuilderOptions.IgnoreProperNouns;
            _ignoreAramaic = deckBuilderOptions.IgnoreAramaic;
            _minimumNumberOfOccurrences = deckBuilderOptions.MinimumNumberOfOccurrences;
            _maximumNumberOfOccurrences = deckBuilderOptions.MaximumNumberOfOccurrences;

            try
            {
                var totalWordOccurrencesJson = File.ReadAllText(_totalWordOccurrencesJsonPath);
                _totalWordOccurrences = JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, Dictionary<string, int>>>>(totalWordOccurrencesJson)!;
            }
            catch
            {
                _totalWordOccurrencesNeedsUpdated = true;
            }
        }

        public List<Deck> Build(List<Chapter> chaptersToBuild = null, List<(string, string)> wordsToSkip = null, DeckScope deckScope = DeckScope.Chapter)
        {
            chaptersToBuild = chaptersToBuild ?? BookData.GetAllChapters();
            wordsToSkip = wordsToSkip ?? new();
            
            if (_totalWordOccurrencesNeedsUpdated)
                UpdateTotalWordsOccurrences();
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
                            Name = $"{bookDeckNamePrefix}::Chapter {chapterIndex.ToString("000")}",
                        };

                    foreach (var wlcWord in chapter.Verses.SelectMany(v => v.Words))
                    {
                        var entry = GetLexicalIndexEntry(wlcWord.Lemma);

                        if (_ignoreProperNouns && entry.Pos == "Np")
                            continue;
                        if (_ignoreAramaic && entry.LanguageCode == "arc")
                            continue;
                        if (wordsToSkip.Any(s => s.Item1 == entry.Word && s.Item2 == entry.LanguageCode))
                            continue;

                        var totalWordOccurrences = _totalWordOccurrences
                                [entry.LanguageCode]
                            [entry.Word]
                            .Values
                            .Sum(v => v);
                        if (totalWordOccurrences < _minimumNumberOfOccurrences
                            || totalWordOccurrences > _maximumNumberOfOccurrences)
                            continue;

                        var definitionIndex = 1;
                        var questionDefinitionList = new List<string>();
                        var answerDefinitionList = new List<string>();
                        var lexicalIndexEntries = _lexicalIndexEntries.Where(e => e.Word == entry.Word && e.LanguageCode == entry.LanguageCode).ToList();

                        foreach (var lexicalEntry in lexicalIndexEntries
                                     .Where(e => _totalWordOccurrences[entry.LanguageCode][entry.Word].TryGetValue(GetSimplifiedLemma(e), out _))
                                     .OrderByDescending(e =>
                                     _totalWordOccurrences[entry.LanguageCode][entry.Word][GetSimplifiedLemma(e)]))
                        {
                            var bdbEntry = _bdbEntries.First(b => b.Id == lexicalEntry.BdbIndex);

                            questionDefinitionList.Add($"{definitionIndex}. {bdbEntry.DefinitionsForQuestion}");
                            answerDefinitionList.Add($"{definitionIndex}. {bdbEntry.DefinitionsForAnswer}");
                            definitionIndex++;
                        }
                        var definitionForQuestion = string.Join(" <br /> ", questionDefinitionList);
                        var definitionForAnswer = string.Join(" <br /> ", answerDefinitionList);

                        var existingNote = deck.Notes.FirstOrDefault(n => n.Word == entry.Word && n.IsHebrew == (entry.LanguageCode == "heb"))
                            ?? decks.SelectMany(d => d.Notes).FirstOrDefault(n => n.Word == entry.Word && n.IsHebrew == (entry.LanguageCode == "heb"));

                        if (existingNote == null)
                            deck.Notes.Add(new Note
                            {
                                Word = entry.Word,
                                DefinitionForQuestion = definitionForQuestion,
                                DefinitionForAnswer = definitionForAnswer,
                                TotalOccurrences = totalWordOccurrences,
                                IsHebrew = entry.LanguageCode == "heb"
                            });
                    }

                    if (deckScope == DeckScope.Chapter)
                        decks.Add(deck);
                }

                if (deckScope == DeckScope.Book)
                    decks.Add(bookDeck);
            }
            
            return decks;
        }

        private void UpdateTotalWordsOccurrences()
        {
            string chainedStrongsIndex = null;
            
            foreach (var wlcWord in
                     _wlcBooks.SelectMany(b => b.Chapters)
                         .SelectMany(c => c.Verses)
                         .SelectMany(v => v.Words))
            {
                // only necessary if there's a mistake where a chain doesn't complete
                if (wlcWord.Lemma.Contains("+"))
                {
                    if (chainedStrongsIndex != null
                        && GetStrongsIndexFromLemma(wlcWord.Lemma) != chainedStrongsIndex)
                        throw new InvalidDataException($"{GetStrongsIndexFromLemma(wlcWord.Lemma)} follows {chainedStrongsIndex} and does not match.");

                    chainedStrongsIndex = GetStrongsIndexFromLemma(wlcWord.Lemma);

                    continue;
                }

                chainedStrongsIndex = null;
                var entry = GetLexicalIndexEntry(wlcWord.Lemma);
                            
                UpdateTotalWordsOccurrences(entry.LanguageCode, entry.Word, wlcWord.Lemma);
            }

            var totalWordOccurrencesJson = JsonSerializer.Serialize(_totalWordOccurrences);
            File.Delete(_totalWordOccurrencesJsonPath);
            File.WriteAllText(_totalWordOccurrencesJsonPath, totalWordOccurrencesJson);
        }

        private void UpdateTotalWordsOccurrences(string language, string word, string lemma)
        {
            if (!_totalWordOccurrences.ContainsKey(language))
                _totalWordOccurrences.Add(language, new Dictionary<string, Dictionary<string, int>>());
            
            if (!_totalWordOccurrences[language].ContainsKey(word))
                _totalWordOccurrences[language].Add(word, new Dictionary<string, int>());

            var simplifiedLemma = GetSimplifiedLemma(lemma);
            
            if (!_totalWordOccurrences[language][word].ContainsKey(simplifiedLemma))
                _totalWordOccurrences[language][word].Add(simplifiedLemma, 1);
            else _totalWordOccurrences[language][word][simplifiedLemma]++;
        }

        private LexicalIndexEntry GetLexicalIndexEntry(string lemma)
        {
            var workingLemma = new string(lemma);
            workingLemma = workingLemma.Replace("+", "");
            
            var strong = "";
            string aug = null;

            while (workingLemma.Contains("/"))
                workingLemma = workingLemma.Substring(2);

            if (workingLemma.Contains(" "))
            {
                aug = workingLemma[workingLemma.Length - 1].ToString();
                strong = workingLemma.Substring(0, workingLemma.Length - 2);
            }
            else
                strong = workingLemma;
            
            var result = _lexicalIndexEntries.FirstOrDefault(e => e.StrongsIndex == strong && e.Aug == aug);

            if (result != null)
                return result;

            var options = _lexicalIndexEntries.Where(e => e.StrongsIndex == strong);
            
            if (options.Select(o => o.Word).Distinct().Count() > 1)
                switch (strong)
                {
                    case "7451":
                        break;
                    case "3651":
                        return _lexicalIndexEntries.First(e => e.StrongsIndex == strong && e.Aug == "c");
                    case "6887":
                        return _lexicalIndexEntries.First(e => e.StrongsIndex == strong && e.Aug == "c");
                    default:
                        throw new InvalidOperationException($"Strongs {strong} has not been accounted for for a null aug value.");
                }
                
            return _lexicalIndexEntries.First(e => e.StrongsIndex == strong && e.Aug == "a");
        }

        private string GetStrongsIndexFromLemma(string lemma)
        {
            var workingLemma = new string(lemma);
            workingLemma = GetSimplifiedLemma(workingLemma);

            if (workingLemma.Contains(" "))
                workingLemma = workingLemma.Substring(0, workingLemma.Length - 2);

            return workingLemma;
        }

        private string GetSimplifiedLemma(string lemma)
        {
            var workingLemma = new string(lemma);
            workingLemma = workingLemma.Replace("+", "");
            
            while (workingLemma.Contains("/"))
                workingLemma = workingLemma.Substring(2);

            return workingLemma;
        }

        private string GetSimplifiedLemma(LexicalIndexEntry entry)
        {
            var result = new string(entry.StrongsIndex);
            
            if (!string.IsNullOrWhiteSpace(entry.Aug))
                result += $" {entry.Aug}";
            
            return result;
        }
    }
}