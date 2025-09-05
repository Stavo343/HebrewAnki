using HebrewAnki.Data.Models;

namespace HebrewAnki.Console;

public class DeckBuilderOptions
{
    public List<LexicalIndexEntry> LexicalIndexEntries { get; set; } = new ();
    
    public List<WlcBook> WlcBooks { get; set; } = new ();
    
    public List<BdbEntry> BdbEntries { get; set; } = new ();
    
    public string GlobalDeckNamePrefix { get; set; } = string.Empty;
    
    public bool IgnoreProperNouns { get; set; } = false;
    
    public bool IgnoreAramaic { get; set; } = false;
    
    public int MinimumNumberOfOccurrences { get; set; } = 0;
    
    public int MaximumNumberOfOccurrences { get; set; } = 11871;
}