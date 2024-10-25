namespace HebrewAnki.Data.Models
{
    public class WlcVerse
    {
        public string OsisId { get; set; }

        public List<WlcWord> Words { get; set; } = new();
    }
}
