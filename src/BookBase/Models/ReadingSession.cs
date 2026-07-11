using SQLite;

namespace BookBase.Models;

public sealed class ReadingSession : BaseEntity
{
    [Indexed]
    public int BookId { get; set; }
    public DateTimeOffset StartTime { get; set; }
    public DateTimeOffset EndTime { get; set; }
    public int PagesRead { get; set; }
}
