using BookBase.Interfaces;

namespace BookBase.Services;

/// <summary>
/// Fallback implementation used on platforms that do not have a
/// platform-specific on-device OCR service registered.  Always returns an
/// empty list.
/// </summary>
internal sealed class UnsupportedIsbnTextRecognitionService : IIsbnTextRecognitionService
{
    public Task<IReadOnlyList<string>> RecognizeIsbnCandidatesAsync(
        Stream imageStream,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult<IReadOnlyList<string>>([]);
    }
}
