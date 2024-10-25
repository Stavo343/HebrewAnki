namespace HebrewAnki.Console
{
    public class Deck
    {
        public string Name { get; set; }

        public List<Note> Notes { get; set; } = new();
    }
}
