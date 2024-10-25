namespace HebrewAnki.Console.Models
{
    public class PureWord
    {
        public string Value { get; set; }

        public string RootWord { get; set; }

        public Dictionary<string, int> ChapterOccurrences { get; set; }

        public string MorphologyCode { get; set; }
    }
}
