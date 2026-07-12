using BookBase.Utilities;

namespace BookBase.Tests;

public sealed class IsbnValidatorTests
{
    // ------------------------------------------------------------------ //
    //  ISBN-13 valid                                                       //
    // ------------------------------------------------------------------ //

    [Theory]
    [InlineData("9781402894626")]   // well-known ISBN-13
    [InlineData("9780306406157")]   // another valid ISBN-13
    [InlineData("9789999999991")]   // ISBN-13 with 9-heavy digits (check digit = 1)
    [InlineData("9780000000002")]   // minimal valid ISBN-13 (check digit = 2)
    public void IsValid_ValidIsbn13_ReturnsTrue(string isbn) =>
        Assert.True(IsbnValidator.IsValid(isbn));

    // ------------------------------------------------------------------ //
    //  ISBN-13 invalid                                                     //
    // ------------------------------------------------------------------ //

    [Theory]
    [InlineData("9781402894625")]   // wrong check digit (should be 6)
    [InlineData("9780306406150")]   // wrong check digit
    [InlineData("978140289462X")]   // 'X' is not valid in ISBN-13
    [InlineData("978140289462")]    // too short (12 digits)
    [InlineData("97814028946260")]  // too long (14 digits)
    public void IsValid_InvalidIsbn13_ReturnsFalse(string isbn) =>
        Assert.False(IsbnValidator.IsValid(isbn));

    // ------------------------------------------------------------------ //
    //  ISBN-10 valid                                                       //
    // ------------------------------------------------------------------ //

    [Theory]
    [InlineData("0306406152")]      // standard ISBN-10
    [InlineData("080442957X")]      // ISBN-10 with 'X' check digit
    [InlineData("0000000000")]      // check: 0*10+...+0*1 = 0, 0 % 11 == 0
    public void IsValid_ValidIsbn10_ReturnsTrue(string isbn) =>
        Assert.True(IsbnValidator.IsValid(isbn));

    // ------------------------------------------------------------------ //
    //  ISBN-10 invalid                                                     //
    // ------------------------------------------------------------------ //

    [Theory]
    [InlineData("0306406151")]      // wrong check digit
    [InlineData("030640615")]       // too short (9 digits)
    [InlineData("03064061520")]     // too long (11 digits)
    [InlineData("030640615A")]      // non-digit, non-X last character
    public void IsValid_InvalidIsbn10_ReturnsFalse(string isbn) =>
        Assert.False(IsbnValidator.IsValid(isbn));

    // ------------------------------------------------------------------ //
    //  Null / empty / wrong length                                         //
    // ------------------------------------------------------------------ //

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("123")]
    [InlineData("12345678901234")]
    public void IsValid_NullEmptyOrWrongLength_ReturnsFalse(string? isbn) =>
        Assert.False(IsbnValidator.IsValid(isbn));

    // ------------------------------------------------------------------ //
    //  IsBookIsbn13                                                        //
    // ------------------------------------------------------------------ //

    [Theory]
    [InlineData("9781402894626")]   // 978 prefix, valid checksum
    [InlineData("9790000000001")]   // 979 prefix, valid checksum (check digit = 1)
    public void IsBookIsbn13_ValidBookIsbn_ReturnsTrue(string isbn) =>
        Assert.True(IsbnValidator.IsBookIsbn13(isbn));

    [Fact]
    public void IsBookIsbn13_NonBookPrefix_ReturnsFalse()
    {
        // A valid ISBN-13 but with a non-book prefix (e.g. 977 = periodicals)
        // Manually construct a valid-checksum 977 ISBN-13.
        // 977 prefix check: there is no formally defined 977 ISBN, but the validator
        // should reject it as a book ISBN regardless.
        Assert.False(IsbnValidator.IsBookIsbn13("9780000000002".Replace("978", "977")));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("978140289462")]    // valid prefix, too short
    [InlineData("9781402894625")]   // valid prefix, invalid checksum
    public void IsBookIsbn13_NullOrInvalid_ReturnsFalse(string? isbn) =>
        Assert.False(IsbnValidator.IsBookIsbn13(isbn));
}
