using BookBase.Interfaces;

namespace BookBase.Services;

/// <summary>
/// Recognition strategy wrapper for printed ISBN extraction. This currently
/// delegates to <see cref="IIsbnTextRecognitionService"/> so scanner flows can
/// depend on a strategy-specific contract while keeping the OCR implementation
/// platform-specific.
/// </summary>
public sealed class PrintedIsbnRecognitionService : IPrintedIsbnRecognitionService
{
    private readonly IIsbnTextRecognitionService _textRecognitionService;

    public PrintedIsbnRecognitionService(IIsbnTextRecognitionService textRecognitionService)
    {
        _textRecognitionService = textRecognitionService;
    }

    public Task<IReadOnlyList<string>> RecognizeIsbnCandidatesAsync(
        Stream imageStream,
        CancellationToken cancellationToken = default)
    {
        return _textRecognitionService.RecognizeIsbnCandidatesAsync(imageStream, cancellationToken);
    }
}
