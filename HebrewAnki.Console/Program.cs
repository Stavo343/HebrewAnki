using AnkiNet;
using HebrewAnki.Console;
using HebrewAnki.Console.Enums;
using HebrewAnki.Data.XmlParsers;

var lexicalIndexEntries = LexicalIndexParser.ParseLexicalIndex();
var wlcBooks = WlcParser.GetWlcBooks();
var oshmEntries = OshmParser.GetOshmEntries();

var deckScope = DeckScope.Book;
var decks = new DeckBuilder(lexicalIndexEntries, wlcBooks, oshmEntries).Build(deckScope);

var collection = CollectionBuilder.Build(decks);

await AnkiFileWriter.WriteToFileAsync($"../../../output/Hebrew Bible Vocab by {deckScope.ToString()}.apkg", collection);