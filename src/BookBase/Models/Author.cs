using SQLite;

namespace BookBase.Models;

public sealed class Author : BaseEntity
{
    [Indexed(Unique = true)]
    public string Name { get; set; } = string.Empty;
    public string? Biography { get; set; }
}
