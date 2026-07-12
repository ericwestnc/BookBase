#if ANDROID
using Android.Graphics;
using Android.Gms.Extensions;
using BookBase.Interfaces;
using BookBase.Utilities;
using Xamarin.Google.MLKit.Vision.Common;
using Xamarin.Google.MLKit.Vision.Text;
using Xamarin.Google.MLKit.Vision.Text.Latin;

namespace BookBase.Platforms.Android;

/// <summary>
/// On-device ISBN text recognition for Android using Google ML Kit
/// Text Recognition (Latin script).  The ML model is downloaded once on
/// first use and then executed entirely on-device with no network access.
/// </summary>
internal sealed class AndroidIsbnTextRecognitionService : IIsbnTextRecognitionService
{
    public async Task<IReadOnlyList<string>> RecognizeIsbnCandidatesAsync(
        Stream imageStream,
        CancellationToken cancellationToken = default)
    {
        Bitmap? bitmap = null;
        try
        {
            bitmap = await BitmapFactory.DecodeStreamAsync(imageStream);
            if (bitmap is null)
            {
                return [];
            }

            var inputImage = InputImage.FromBitmap(bitmap, 0);

            // TextRecognizerOptions.DefaultOptions targets the Latin script model.
            using var recognizer = TextRecognition.GetClient(TextRecognizerOptions.DefaultOptions);
            var result = await recognizer.Process(inputImage).AsAsync<Text>();

            return result is null
                ? []
                : IsbnExtractor.ExtractIsbnCandidates(result.GetText());
        }
        finally
        {
            bitmap?.Recycle();
        }
    }
}
#endif
