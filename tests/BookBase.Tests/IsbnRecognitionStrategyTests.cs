using BookBase.Interfaces;
using BookBase.Services;

namespace BookBase.Tests;

public sealed class IsbnRecognitionStrategyTests
{
    [Fact]
    public void ManualEntryService_NormalizesAndValidatesIsbn()
    {
        var service = new ManualEntryService();

        var normalized = service.TryNormalize("978-1-4028-9462-6");

        Assert.Equal("9781402894626", normalized);
    }

    [Fact]
    public void ManualEntryService_ReturnsNullForInvalidInput()
    {
        var service = new ManualEntryService();

        var normalized = service.TryNormalize("not-an-isbn");

        Assert.Null(normalized);
    }

    [Fact]
    public async Task PrintedIsbnRecognitionService_UsesTextRecognitionResults()
    {
        var expected = new[] { "9781402894626", "1402894627" };
        var service = new PrintedIsbnRecognitionService(new StubTextRecognitionService(expected));

        await using var stream = new MemoryStream([1, 2, 3]);
        var candidates = await service.RecognizeIsbnCandidatesAsync(stream);

        Assert.Equal(expected, candidates);
    }

    private sealed class StubTextRecognitionService(IReadOnlyList<string> candidates) : IIsbnTextRecognitionService
    {
        public Task<IReadOnlyList<string>> RecognizeIsbnCandidatesAsync(
            Stream imageStream,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(candidates);
        }
    }
}
