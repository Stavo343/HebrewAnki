using System.Text;
using System.Text.RegularExpressions;

namespace HebrewAnki.Data;

public static class HebrewStringHelper
{
    // Precompiled regex for Hebrew cantillation marks (U+0591–U+05AF)
    private static readonly Regex CantillationRegex = new Regex(
        @"\u0591-\u05AF",
        RegexOptions.Compiled | RegexOptions.CultureInvariant
    );

    /// <summary>
    /// Removes Hebrew cantillation marks and normalizes to NFC for comparison.
    /// Optimized for large texts using precompiled regex.
    /// </summary>
    /// <param name="input">Input string with potential cantillation marks</param>
    /// <returns>Normalized string without cantillation marks</returns>
    public static string CleanAndNormalize(string input)
    {
        if (string.IsNullOrEmpty(input))
            return input;

        // Remove cantillation marks
        string cleaned = CantillationRegex.Replace(input, string.Empty);

        // Normalize to NFC
        return cleaned.Normalize(NormalizationForm.FormC);
    }
}