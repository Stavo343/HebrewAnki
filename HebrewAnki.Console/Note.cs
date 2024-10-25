namespace HebrewAnki.Console
{
    public class Note
    {
        public string PrintedText { get; set; }

        public string Root { get; set; }

        public string Definition { get; set; }

        public string Oshm {  get; set; }

        public int TotalOccurrences { get; set; } = 0;

        public int TotalRootOccurrences { get; set; } = 0;

        public bool IsHebrew => Oshm.Contains("Hebrew");
    }
}
