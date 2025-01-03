using AnkiNet;
using System.Web;

namespace HebrewAnki.Console
{
    public static class CollectionBuilder
    {
        private static readonly long _hebrewNoteTypeId = 1734550203314;
        private static readonly long _aramaicNoteTypeId = 1734550280545;
        
        public static AnkiCollection Build(List<Deck> decks)
        {
            var collection = new AnkiCollection();

            var hebrewToEnglishCardType = new AnkiCardType
            {
                Name = "Hebrew to English",
                QuestionFormat = File.ReadAllText("../HebrewAnki.Console/html/HebrewToEnglishQuestionFormat.html"),
                AnswerFormat = File.ReadAllText("../HebrewAnki.Console/html/HebrewToEnglishAnswerFormat.html"),
                Ordinal = 0,
            };

            var englishToHebrewCardType = new AnkiCardType
            {
                Name = "English to Hebrew",
                QuestionFormat = File.ReadAllText("../HebrewAnki.Console/html/EnglishToHebrewQuestionFormat.html"),
                AnswerFormat = File.ReadAllText("../HebrewAnki.Console/html/EnglishToHebrewAnswerFormat.html"),
                Ordinal = 1,
            };

            var hebrewNoteType = new AnkiNoteType(
                "Hebrew Vocab Per Chapter",
                [hebrewToEnglishCardType, englishToHebrewCardType],
                ["Hebrew Word", "Definition", /*"Variations Within Chapter", */"Total Occurrences", "Deck Information"],
                File.ReadAllText("../HebrewAnki.Console/html/css/hebrew.css"));
            hebrewNoteType.Id = _hebrewNoteTypeId;

            var aramaicToEnglishCardType = new AnkiCardType
            {
                Name = "Aramaic to English",
                QuestionFormat = File.ReadAllText("../HebrewAnki.Console/html/AramaicToEnglishQuestionFormat.html"),
                AnswerFormat = File.ReadAllText("../HebrewAnki.Console/html/AramaicToEnglishAnswerFormat.html"),
                Ordinal = 0,
            };

            var englishToAramaicCardType = new AnkiCardType
            {
                Name = "English to Aramaic",
                QuestionFormat = File.ReadAllText("../HebrewAnki.Console/html/EnglishToAramaicQuestionFormat.html"),
                AnswerFormat = File.ReadAllText("../HebrewAnki.Console/html/EnglishToAramaicAnswerFormat.html"),
                Ordinal = 1,
            };

            var aramaicNoteType = new AnkiNoteType(
                "Aramaic Vocab Per Chapter",
                [aramaicToEnglishCardType, englishToAramaicCardType],
                ["Aramaic Word", "Definition", /*"Variations Within Chapter", */"Total Occurrences", "Deck Information"],
                File.ReadAllText("../HebrewAnki.Console/html/css/aramaic.css"));
            aramaicNoteType.Id = _aramaicNoteTypeId;

            collection.CreateNoteType(hebrewNoteType, _hebrewNoteTypeId);
            collection.CreateNoteType(aramaicNoteType, _aramaicNoteTypeId);

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
                            ? _hebrewNoteTypeId
                            : _aramaicNoteTypeId,
                        tagsString,
                        note.Word,
                        HttpUtility.HtmlEncode(note.Definition),
                        //string.Join(" <br /> ", note.Variations.Select(v => $"- {v.Variation}: {v.Oshm}")),
                        note.TotalOccurrences.ToString(),
                        string.Empty
                        );
                }
            }

            return collection;
        }
    }
}
