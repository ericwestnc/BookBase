using System.Text.RegularExpressions;

namespace BookBase.Utilities;

/// <summary>
/// Extracts and validates ISBN candidates from a block of recognized text.
/// </summary>
internal static partial class IsbnExtractor
{
    // Matches digit-only runs of 10 or 13 digits, optionally preceded by an
    // "ISBN" label and optionally containing hyphens or spaces as separators.
    // Examples matched: "9781402894626", "978-1-4028-9462-6", "ISBN 0-306-40615-2"
    [GeneratedRegex(
        @"(?:ISBN[-: ]?)?"           +  // optional label
        @"(97[89][- ]?(?:\d[- ]?){9}\d" +  // ISBN-13 starting with 978/979
        @"|(?:\d[- ]?){9}[\dXx])",         // ISBN-10
        RegexOptions.IgnoreCase)]
    private static partial Regex IsbnPattern();

    /// <summary>
    /// Scans <paramref name="text"/> and returns all distinct, checksum-valid
    /// ISBN-10 and ISBN-13 candidates, in the order they appear.
    /// </summary>
    public static IReadOnlyList<string> ExtractIsbnCandidates(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return [];
        }

        var seen = new HashSet<string>(StringComparer.Ordinal);
        var results = new List<string>();

        foreach (Match m in IsbnPattern().Matches(text))
        {
            // Group 1 contains only the digit sequence (without any "ISBN" label).
            var raw = m.Groups[1].Value;
            var normalized = IsbnNormalizer.Normalize(raw);
            if (normalized.Length is 10 or 13
                && IsbnValidator.IsValid(normalized)
                && seen.Add(normalized))
            {
                results.Add(normalized);
            }
        }

        return results;
    }
}
