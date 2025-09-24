using AnkiNet;
using System.Web;
using HebrewAnki.Data;

namespace HebrewAnki
{
    public static class CollectionBuilder
    {
        public static AnkiCollection Build(List<Deck> decks, AnkiCollection existingCollection = null)
        {
            var collection = existingCollection ?? new AnkiCollection();

            if (existingCollection == null)
                CreateNoteTypes(collection);

            foreach (var deck in decks)
            {
                collection.TryGetDeckByName(deck.Name, out var existingDeck);
                long deckId;
                if (existingDeck.Id == 0)
                    deckId = collection.CreateDeck(deck.Name);
                else
                    deckId = existingDeck.Id;

                foreach (var note in deck.Notes)
                {
                    var tags = new List<string>();

                    if (note.TotalOccurrences > 70)
                        tags.Add("MoreThanSeventyOccurrences");

                    if (note.TotalOccurrences < 5)
                        tags.Add("FewerThanFiveOccurrences");

                    if (note.DefinitionForQuestion[0].ToString() == note.DefinitionForQuestion[0].ToString().ToUpper())
                        tags.Add("ProperNoun");

                    if (!note.IsHebrew)
                        tags.Add("Aramaic");

                    var tagsString = tags.Any()
                        ? $" {string.Join(" ", tags)} "
                        : string.Empty;

                    var existingNote = collection.Decks.SelectMany(d => d.Cards).Select(c => c.Note).Distinct()
                        .FirstOrDefault(n => HebrewStringHelper.CleanAndNormalize(n.Fields[0]) == HebrewStringHelper.CleanAndNormalize(note.Word)
                                        && note.IsHebrew == (n.NoteTypeId == AnkiConstants.HebrewNoteTypeId));

                    if (existingNote.Id == 0)
                        collection.CreateNote(
                            deckId,
                            note.Guid,
                            note.IsHebrew
                                ? AnkiConstants.HebrewNoteTypeId
                                : AnkiConstants.AramaicNoteTypeId,
                            tagsString,
                            note.Word,
                            HttpUtility.HtmlEncode(note.DefinitionForQuestion),
                            HttpUtility.HtmlEncode(note.DefinitionForAnswer),
                            note.TotalOccurrences.ToString()
                            );
                    else
                        existingNote.Fields[2] = HttpUtility.HtmlEncode(note.DefinitionForAnswer);
                }
            }

            return collection;
        }

        private static void CreateNoteTypes(AnkiCollection collection)
        {
            var hebrewToEnglishCardType = new AnkiCardType
            {
                Name = "Hebrew to English",
                QuestionFormat = File.ReadAllText($"{AppContext.BaseDirectory}/html/HebrewToEnglishQuestionFormat.html"),
                AnswerFormat = File.ReadAllText($"{AppContext.BaseDirectory}/html/HebrewToEnglishAnswerFormat.html"),
                Ordinal = 0,
            };

            var englishToHebrewCardType = new AnkiCardType
            {
                Name = "English to Hebrew",
                QuestionFormat = File.ReadAllText($"{AppContext.BaseDirectory}/html/EnglishToHebrewQuestionFormat.html"),
                AnswerFormat = File.ReadAllText($"{AppContext.BaseDirectory}/html/EnglishToHebrewAnswerFormat.html"),
                Ordinal = 1,
            };

            var hebrewNoteType = new AnkiNoteType(
                "Hebrew Vocab Per Chapter",
                [hebrewToEnglishCardType, englishToHebrewCardType],
                ["Hebrew Word", "DefinitionForQuestion", "DefinitionForAnswer", "Total Occurrences"],
                File.ReadAllText($"{AppContext.BaseDirectory}/html/css/hebrew.css"));
            hebrewNoteType.Id = AnkiConstants.HebrewNoteTypeId;

            var aramaicToEnglishCardType = new AnkiCardType
            {
                Name = "Aramaic to English",
                QuestionFormat = File.ReadAllText($"{AppContext.BaseDirectory}/html/AramaicToEnglishQuestionFormat.html"),
                AnswerFormat = File.ReadAllText($"{AppContext.BaseDirectory}/html/AramaicToEnglishAnswerFormat.html"),
                Ordinal = 0,
            };

            var englishToAramaicCardType = new AnkiCardType
            {
                Name = "English to Aramaic",
                QuestionFormat = File.ReadAllText($"{AppContext.BaseDirectory}/html/EnglishToAramaicQuestionFormat.html"),
                AnswerFormat = File.ReadAllText($"{AppContext.BaseDirectory}/html/EnglishToAramaicAnswerFormat.html"),
                Ordinal = 1,
            };

            var aramaicNoteType = new AnkiNoteType(
                "Aramaic Vocab Per Chapter",
                [aramaicToEnglishCardType, englishToAramaicCardType],
                ["Aramaic Word", "DefinitionForQuestion", "DefinitionForAnswer", "Total Occurrences"],
                File.ReadAllText($"{AppContext.BaseDirectory}/html/css/aramaic.css"));
            aramaicNoteType.Id = AnkiConstants.AramaicNoteTypeId;

            collection.CreateNoteType(hebrewNoteType, AnkiConstants.HebrewNoteTypeId);
            collection.CreateNoteType(aramaicNoteType, AnkiConstants.AramaicNoteTypeId);
        }
    }
}