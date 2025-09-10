namespace HebrewAnki.Console
{
    public static class BookData
    {
        public static readonly Dictionary<string, int> ChaptersPerBook = new()
        {
            { "בראשית", 50 },
            { "שמות", 40 },
            { "ויקרא", 27 },
            { "במדבר", 36 },
            { "דברים", 34 },
            { "יהושע", 24 },
            { "שופטים", 21 },
            { "שמואל א", 31 },
            { "שמואל ב", 24 },
            { "מלכים א", 22 },
            { "מלכים ב", 25 },
            { "ישעיהו", 66 },
            { "ירמיהו", 52 },
            { "יחזקאל", 48 },
            { "הושע", 14 },
            { "יואל", 4 },
            { "עמוס", 9 },
            { "עובדיה", 1 },
            { "יונה", 4 },
            { "מיכה", 7 },
            { "נחום", 3 },
            { "חבקוק", 3 },
            { "צפניה", 3 },
            { "חגי", 2 },
            { "זכריה", 14 },
            { "מלאכי", 3 },
            { "תהלים", 150 },
            { "איוב", 42 },
            { "משלי", 31 },
            { "שיר השירים", 8 },
            { "רות", 4 },
            { "קהלת", 12 },
            { "איכה", 5 },
            { "אסתר", 10 },
            { "דניאל", 12 },
            { "עזרא", 10 },
            { "נחמיה", 13 },
            { "דברי הימים א", 29 },
            { "דברי הימים ב", 36 }
        };
    
        public static readonly Dictionary<string, string> WlcBookHebrewNames = new()
            {
                { "Gen", "בראשית" },
                { "Exod", "שמות" },
                { "Lev", "ויקרא" },
                { "Num", "במדבר" },
                { "Deut", "דברים" },
                { "Josh", "יהושע" },
                { "Judg", "שופטים" },
                { "1Sam", "שמואל א" },
                { "2Sam", "שמואל ב" },
                { "1Kgs", "מלכים א" },
                { "2Kgs", "מלכים ב" },
                { "Isa", "ישעיהו" },
                { "Jer", "ירמיהו" },
                { "Ezek", "יחזקאל" },
                { "Hos", "הושע" },
                { "Joel", "יואל" },
                { "Amos", "עמוס" },
                { "Obad", "עובדיה" },
                { "Jonah", "יונה" },
                { "Mic", "מיכה" },
                { "Nah", "נחום" },
                { "Hab", "חבקוק" },
                { "Zeph", "צפניה" },
                { "Hag", "חגי" },
                { "Zech", "זכריה" },
                { "Mal", "מלאכי" },
                { "Ps", "תהלים" },
                { "Job", "איוב" },
                { "Prov", "משלי" },
                { "Song", "שיר השירים" },
                { "Ruth", "רות" },
                { "Eccl", "קהלת" },
                { "Lam", "איכה" },
                { "Esth", "אסתר" },
                { "Dan", "דניאל" },
                { "Ezra", "עזרא" },
                { "Neh", "נחמיה" },
                { "1Chr", "דברי הימים א" },
                { "2Chr", "דברי הימים ב" }
            };

        public static List<Chapter> GetAllChapters()
        {
            var result = new List<Chapter>();
        
            foreach (var bookName in ChaptersPerBook.Keys)
                for (var i = 1; i < ChaptersPerBook[bookName] + 1; i++)
                    result.Add(new Chapter
                    {
                        Book = bookName,
                        ChapterNumber = i,
                    });

            return result;
        }
    }
}