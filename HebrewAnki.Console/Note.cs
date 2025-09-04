namespace HebrewAnki.Console
{
    public class Note
    {
        public string Word { get; set; }

        public string DefinitionForQuestion { get; set; }

        public string DefinitionForAnswer { get; set; }

        //public List<WordVariation> Variations { get; set; } = new();

        public int TotalOccurrences { get; set; } = 0;

        public bool IsHebrew { get; set; } = true;
    }
}
