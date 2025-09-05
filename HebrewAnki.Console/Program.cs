using AnkiNet;
using HebrewAnki.Console;
using HebrewAnki.Console.Enums;
using HebrewAnki.Data.XmlParsers;

var lexicalIndexEntries = LexicalIndexParser.ParseLexicalIndex("../../../../HebrewAnki.Data/lexicon/LexicalIndex.xml");
var bdbEntries = BdbParser.ParseBdb("../../../../HebrewAnki.Data/lexicon/bdb.xml");
var wlcBooks = WlcParser.ParseWlcBooks("../../../../HebrewAnki.Data/wlc/");

var deckScope = DeckScope.Book;
var globalDeckNamePrefix = $"11/21/24: Hebrew/Aramaic Vocab Per {deckScope.ToString()}";