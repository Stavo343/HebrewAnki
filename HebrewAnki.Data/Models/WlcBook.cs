namespace HebrewAnki.Data.Models
{
    public class WlcBook
    {
        public string OsisId { get; set; }

        public List<WlcChapter> Chapters { get; set; } = new();
    }
}
