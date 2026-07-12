using BookBase.Utilities;

namespace BookBase.Tests;

public sealed class IsbnExtractorTests
{
    [Fact]
    public void ExtractIsbnCandidates_PureIsbn13_ReturnsThatIsbn()
    {
        var result = IsbnExtractor.ExtractIsbnCandidates("9781402894626");
        Assert.Single(result);
        Assert.Equal("9781402894626", result[0]);
    }

    [Fact]
    public void ExtractIsbnCandidates_HyphenatedIsbn13_ReturnsNormalized()
    {
        var result = IsbnExtractor.ExtractIsbnCandidates("978-1-4028-9462-6");
        Assert.Single(result);
        Assert.Equal("9781402894626", result[0]);
    }

    [Fact]
    public void ExtractIsbnCandidates_IsbnLabelPrefix_ReturnsIsbn()
    {
        var result = IsbnExtractor.ExtractIsbnCandidates("ISBN 978-1-4028-9462-6");
        Assert.Single(result);
        Assert.Equal("9781402894626", result[0]);
    }

    [Fact]
    public void ExtractIsbnCandidates_MultipleIsbnsInText_ReturnsAll()
    {
        var text = "First book: 9781402894626 and second: 0306406152";
        var result = IsbnExtractor.ExtractIsbnCandidates(text);
        Assert.Equal(2, result.Count);
        Assert.Contains("9781402894626", result);
        Assert.Contains("0306406152", result);
    }

    [Fact]
    public void ExtractIsbnCandidates_InvalidChecksum_ReturnsEmpty()
    {
        // 9781402894625 has a wrong check digit
        var result = IsbnExtractor.ExtractIsbnCandidates("9781402894625");
        Assert.Empty(result);
    }

    [Fact]
    public void ExtractIsbnCandidates_EmptyText_ReturnsEmpty()
    {
        Assert.Empty(IsbnExtractor.ExtractIsbnCandidates(null));
        Assert.Empty(IsbnExtractor.ExtractIsbnCandidates(""));
        Assert.Empty(IsbnExtractor.ExtractIsbnCandidates("   "));
    }

    [Fact]
    public void ExtractIsbnCandidates_DuplicateIsbn_ReturnsOnce()
    {
        var text = "9781402894626 and again: 9781402894626";
        var result = IsbnExtractor.ExtractIsbnCandidates(text);
        Assert.Single(result);
    }

    [Fact]
    public void ExtractIsbnCandidates_ValidIsbn10WithX_ReturnsIsbn()
    {
        var result = IsbnExtractor.ExtractIsbnCandidates("ISBN 080442957X");
        Assert.Single(result);
        Assert.Equal("080442957X", result[0]);
    }
}
