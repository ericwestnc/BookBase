using SQLite;

namespace BookBase.Models;

public sealed class Tag : BaseEntity
{
    [Indexed(Unique = true)]
    public string Name { get; set; } = string.Empty;
}
