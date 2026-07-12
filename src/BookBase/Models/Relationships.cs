using SQLite;

namespace BookBase.Models;

public sealed class BookAuthor
{
    [Indexed]
    public int BookId { get; set; }
    [Indexed]
    public int AuthorId { get; set; }
}

public sealed class BookGenre
{
    [Indexed]
    public int BookId { get; set; }
    [Indexed]
    public int GenreId { get; set; }
}

public sealed class BookTag
{
    [Indexed]
    public int BookId { get; set; }
    [Indexed]
    public int TagId { get; set; }
}

public sealed class BookCollection
{
    [Indexed]
    public int BookId { get; set; }
    [Indexed]
    public int CollectionId { get; set; }
}
