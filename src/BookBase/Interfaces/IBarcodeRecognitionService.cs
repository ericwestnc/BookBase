using ZXing.Net.Maui;

namespace BookBase.Interfaces;

public interface IBarcodeRecognitionService
{
    string? TryRecognizeIsbn(string rawValue, BarcodeFormat format);
}
