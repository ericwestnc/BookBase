using SQLite;

namespace BookBase.Models;

public sealed class Collection : BaseEntity
{
    [Indexed(Unique = true)]
    public string Name { get; set; } = string.Empty;
}
