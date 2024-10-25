namespace HebrewAnki.Data.Models
{
    public class WlcChapter
    {
        public string OsisId { get; set; }

        public List<WlcVerse> Verses { get; set; } = new();
    }
}
