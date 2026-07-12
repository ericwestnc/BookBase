using SQLite;

namespace BookBase.Models;

public sealed class Quote : BaseEntity
{
    [Indexed]
    public int BookId { get; set; }
    public string Text { get; set; } = string.Empty;
    public string? Location { get; set; }
}
