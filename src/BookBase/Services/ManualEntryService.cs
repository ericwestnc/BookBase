using BookBase.Interfaces;
using BookBase.Utilities;

namespace BookBase.Services;

public sealed class ManualEntryService : IManualEntryService
{
    public string? TryNormalize(string rawValue)
    {
        var normalized = IsbnNormalizer.Normalize(rawValue);
        return IsbnValidator.IsValid(normalized) ? normalized : null;
    }
}
