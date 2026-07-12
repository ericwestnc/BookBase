using SQLite;

namespace BookBase.Models;

public abstract class BaseEntity
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }
}
