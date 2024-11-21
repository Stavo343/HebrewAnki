using AnkiNet;
using HebrewAnki.Console;
using HebrewAnki.Console.Enums;
using HebrewAnki.Data.XmlParsers;

var lexicalIndexEntries = LexicalIndexParser.ParseLexicalIndex();
var bdbEntries = BdbParser.ParseBdb();
var wlcBooks = WlcParser.GetWlcBooks();
var oshmEntries = OshmParser.GetOshmEntries();

var deckScope = DeckScope.Book;
var globalDeckNamePrefix = $"11/21/24: Hebrew/Aramaic Vocab Per {deckScope.ToString()}";
var decks = new DeckBuilder(lexicalIndexEntries, wlcBooks, bdbEntries, oshmEntries, globalDeckNamePrefix).Build(deckScope);

var collection = CollectionBuilder.Build(decks);

await AnkiFileWriter.WriteToFileAsync($"../../../output/11-21-24 - Hebrew Bible Vocab by {deckScope.ToString()}.apkg", collection);