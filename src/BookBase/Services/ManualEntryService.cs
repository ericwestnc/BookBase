using BookBase.Interfaces;
using BookBase.Utilities;

namespace BookBase.Services;

public sealed class ManualEntryService : IManualEntryService
{
    public string? TryNormalize(string rawValue)
    {
        var normalized = IsbnNormalizer.Normalize(rawValue);
        if (string.IsNullOrWhiteSpace(normalized))
        {
            return null;
        }

        return normalized.All(char.IsAsciiDigit) &&
               (normalized.Length == IsbnLengths.Isbn10 || normalized.Length == IsbnLengths.Isbn13)
            ? normalized
            : null;
    }
}
