namespace BookBase.Interfaces;

public interface IPrintedIsbnRecognitionService
{
    Task<IReadOnlyList<string>> RecognizeIsbnCandidatesAsync(
        Stream imageStream,
        CancellationToken cancellationToken = default);
}
