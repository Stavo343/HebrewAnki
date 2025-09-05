using AnkiNet;
using System.Web;

namespace HebrewAnki.Console
{
    public static class CollectionBuilder
    {
        private static readonly long HebrewNoteTypeId = 1734550203314;
        private static readonly long AramaicNoteTypeId = 1734550280545;
        
        public static AnkiCollection Build(List<Deck> decks, AnkiCollection existingCollection = null)
        {
            // part of the problem is that the deck is always new when the deckId needs to match one that already exists.
            // This becomes complicated because the deck needs to already exist in the collection for the card to get added to it
            // I might just need to add in an optional id for decks like I did for notetypes and see if matching that works
            
            var collection = /*existingCollection ?? */new AnkiCollection();

            //if (existingCollection == null)
                CreateNoteTypes(collection);

            foreach (var deck in decks)
            {
                collection.TryGetDeckByName(deck.Name, out var existingDeck);
                long deckId;
                if (existingDeck == null)
                    deckId = existingDeck.Id;
                else
                    deckId = collection.CreateDeck(deck.Name);

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

                    collection.CreateNote(
                        deckId,
                        note.IsHebrew
                            ? HebrewNoteTypeId
                            : AramaicNoteTypeId,
                        tagsString,
                        note.Word,
                        HttpUtility.HtmlEncode(note.DefinitionForQuestion),
                        HttpUtility.HtmlEncode(note.DefinitionForAnswer),
                        note.TotalOccurrences.ToString()
                        );
                }
            }

            return collection;
        }

        private static void CreateNoteTypes(AnkiCollection collection)
        {
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
                ["Hebrew Word", "DefinitionForQuestion", "DefinitionForAnswer", /*"Variations Within Chapter", */"Total Occurrences"],
                File.ReadAllText("../HebrewAnki.Console/html/css/hebrew.css"));
            hebrewNoteType.Id = HebrewNoteTypeId;

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
                ["Aramaic Word", "DefinitionForQuestion", "DefinitionForAnswer", /*"Variations Within Chapter", */"Total Occurrences"],
                File.ReadAllText("../HebrewAnki.Console/html/css/aramaic.css"));
            aramaicNoteType.Id = AramaicNoteTypeId;

            collection.CreateNoteType(hebrewNoteType, HebrewNoteTypeId);
            collection.CreateNoteType(aramaicNoteType, AramaicNoteTypeId);
        }
    }
}
