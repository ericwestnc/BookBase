using SQLite;

namespace BookBase.Models;

public sealed class Note : BaseEntity
{
    [Indexed]
    public int BookId { get; set; }
    public string Content { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}
