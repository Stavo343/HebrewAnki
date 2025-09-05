namespace HebrewAnki.Data.Models;

public class Preferences
{
    public bool IgnoreProperNouns { get; set; }
    
    public bool IgnoreAramaic { get; set; }
    
    public int MinimumNumberOfOccurrences { get; set; }
    
    public int MaximumNumberOfOccurrences { get; set; }
    
    public bool BackFillOldChapters { get; set; }
}