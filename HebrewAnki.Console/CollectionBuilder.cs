using AnkiNet;
using System.Web;

namespace HebrewAnki.Console
{
    public static class CollectionBuilder
    {
        public static AnkiCollection Build(List<Deck> decks)
        {
            var collection = new AnkiCollection();

            var hebrewToEnglishCardType = new AnkiCardType
            {
                Name = "Hebrew to English",
                QuestionFormat = File.ReadAllText("../../../html/HebrewToEnglishQuestionFormat.html"),
                AnswerFormat = File.ReadAllText("../../../html/HebrewToEnglishAnswerFormat.html"),
                Ordinal = 0,
            };

            var englishToHebrewCardType = new AnkiCardType
            {
                Name = "English to Hebrew",
                QuestionFormat = File.ReadAllText("../../../html/EnglishToHebrewQuestionFormat.html"),
                AnswerFormat = File.ReadAllText("../../../html/EnglishToHebrewAnswerFormat.html"),
                Ordinal = 1,
            };

            var hebrewNoteType = new AnkiNoteType(
                "Hebrew Vocab Per Chapter",
                [hebrewToEnglishCardType, englishToHebrewCardType],
                ["Hebrew Word", "Definition", "Variations Within Chapter", "Total Occurrences", "Deck Information"],
                File.ReadAllText("../../../html/css/hebrew.css"));

            var aramaicToEnglishCardType = new AnkiCardType
            {
                Name = "Aramaic to English",
                QuestionFormat = File.ReadAllText("../../../html/AramaicToEnglishQuestionFormat.html"),
                AnswerFormat = File.ReadAllText("../../../html/AramaicToEnglishAnswerFormat.html"),
                Ordinal = 0,
            };

            var englishToAramaicCardType = new AnkiCardType
            {
                Name = "English to Aramaic",
                QuestionFormat = File.ReadAllText("../../../html/EnglishToAramaicQuestionFormat.html"),
                AnswerFormat = File.ReadAllText("../../../html/EnglishToAramaicAnswerFormat.html"),
                Ordinal = 1,
            };

            var aramaicNoteType = new AnkiNoteType(
                "Aramaic Vocab Per Chapter",
                [aramaicToEnglishCardType, englishToAramaicCardType],
                ["Aramaic Word", "Definition", "Variations Within Chapter", "Total Occurrences", "Deck Information"],
                File.ReadAllText("../../../html/css/aramaic.css"));

            var hebrewNoteTypeId = collection.CreateNoteType(hebrewNoteType);
            var aramaicNoteTypeId = collection.CreateNoteType(aramaicNoteType);

            foreach (var deck in decks)
            {
                var deckId = collection.CreateDeck(deck.Name);

                foreach (var note in deck.Notes)
                {
                    var tags = new List<string>();

                    if (note.TotalOccurrences > 70)
                        tags.Add("MoreThanSeventyOccurrences");

                    if (note.TotalOccurrences < 5)
                        tags.Add("FewerThanFiveOccurrences");

                    if (note.Definition[0].ToString() == note.Definition[0].ToString().ToUpper())
                        tags.Add("ProperNoun");

                    if (!note.IsHebrew)
                        tags.Add("Aramaic");

                    var tagsString = tags.Any()
                        ? $" {string.Join(" ", tags)} "
                        : string.Empty;

                    collection.CreateNote(
                        deckId,
                        note.IsHebrew
                            ? hebrewNoteTypeId
                            : aramaicNoteTypeId,
                        tagsString,
                        note.Word,
                        HttpUtility.HtmlEncode(note.Definition),
                        string.Join(" <br /> ", note.Variations.Select(v => $"- {v.Variation}: {v.Oshm}")),
                        note.TotalOccurrences.ToString(),
                        string.Empty
                        );
                }
            }

            return collection;
        }
    }
}
