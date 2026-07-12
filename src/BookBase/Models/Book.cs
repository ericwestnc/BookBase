using SQLite;

namespace BookBase.Models;

public sealed class Book : BaseEntity
{
    [Indexed]
    public string? ISBN10 { get; set; }
    [Indexed]
    public string? ISBN13 { get; set; }
    [Indexed]
    public string Title { get; set; } = string.Empty;
    public string? Subtitle { get; set; }
    public string? Description { get; set; }
    public string? Author { get; set; }
    public string? Publisher { get; set; }
    public DateTimeOffset? PublishedDate { get; set; }
    public int? PageCount { get; set; }
    public string? Language { get; set; }
    public BookFormat Format { get; set; }
    public ReadingStatus Status { get; set; }
    public double Rating { get; set; }
    public string? PersonalNotes { get; set; }
    public int CurrentPage { get; set; }
    public int TotalPages { get; set; }
    public DateTimeOffset DateAdded { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? DateStarted { get; set; }
    public DateTimeOffset? DateFinished { get; set; }
    public string? CoverImage { get; set; }
    public string? CoverUrl { get; set; }
    public bool Favorite { get; set; }
    public bool Owned { get; set; } = true;
    public bool Wishlist { get; set; }
    public decimal PricePaid { get; set; }
    public string? LocationOnShelf { get; set; }

    /// <summary>Returns a shallow copy of this <see cref="Book"/> instance.</summary>
    public Book Clone() => (Book)MemberwiseClone();
}
