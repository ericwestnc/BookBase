using BookBase.Interfaces;

namespace BookBase.Services;

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
