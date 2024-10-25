using HebrewAnki.Console;
using HebrewAnki.Data.XmlParsers;

var lexicalIndexEntries = LexicalIndexParser.ParseLexicalIndex();
var wlcBooks = WlcParser.GetWlcBooks();
var oshmEntries = OshmParser.GetOshmEntries();

var deck = new DeckBuilder(lexicalIndexEntries, wlcBooks, oshmEntries).Build();

//AnkiCollection collection = await AnkiFileReader.ReadFromFileAsync(@"C:\Users\ninte\Downloads\Biblical Hebrew Vocabulary (Schwartz-Groves WHV).apkg");
_ = 1;