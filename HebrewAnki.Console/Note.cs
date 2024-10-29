namespace HebrewAnki.Console
{
    public class Note
    {
        public string Word { get; set; }

        public string Definition { get; set; }

        public List<WordVariation> Variations { get; set; } = new();

        public int TotalOccurrences { get; set; } = 0;

        public bool IsHebrew => Variations.First().Oshm.Contains("Hebrew");
    }
}
