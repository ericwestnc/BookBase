using BookBase.Interfaces;
using BookBase.Utilities;
using ZXing.Net.Maui;

namespace BookBase.Services;

public sealed class BarcodeRecognitionService : IBarcodeRecognitionService
{
    public string? TryRecognizeIsbn(string rawValue, BarcodeFormat format)
    {
        var normalized = IsbnNormalizer.Normalize(rawValue);

        if (!IsLikelyBookBarcode(normalized, format))
        {
            return null;
        }

        return IsbnValidator.IsValid(normalized) ? normalized : null;
    }

    private static bool IsLikelyBookBarcode(string normalized, BarcodeFormat format)
    {
        return format switch
        {
            BarcodeFormat.Ean13 =>
                (normalized.StartsWith("978", StringComparison.Ordinal) ||
                 normalized.StartsWith("979", StringComparison.Ordinal)),

            BarcodeFormat.UpcA =>
                normalized.Length == 12 && normalized.All(char.IsAsciiDigit),

            BarcodeFormat.Ean8 =>
                normalized.Length == 8 && normalized.All(char.IsAsciiDigit),

            BarcodeFormat.UpcE =>
                (normalized.Length == 8 || normalized.Length == 12) && normalized.All(char.IsAsciiDigit),

            _ => false
        };
    }
}
