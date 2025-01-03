namespace HebrewAnki.Data.Models
{
    public class LexicalIndexEntry
    {
        public string Word { get; set; }
        
        public string Pos { get; set; }

        public string Definition { get; set; }

        public string BdbIndex { get; set; }

        public string StrongsIndex { get; set; }

        public string? Aug { get; set; }
        
        public string LanguageCode { get; set; }
    }
}
