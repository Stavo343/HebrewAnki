namespace HebrewAnki.Console
{
    public class Note
    {
        public string Guid { get; set; } = System.Guid.NewGuid().ToString().Substring(0, 10);

        public string Word { get; set; }

        public string DefinitionForQuestion { get; set; }

        public string DefinitionForAnswer { get; set; }

        public int TotalOccurrences { get; set; } = 0;

        public bool IsHebrew { get; set; } = true;
    }
}