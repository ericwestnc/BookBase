using BookBase.Models;

namespace BookBase.Interfaces;

public interface IBookLookupService
{
    Task<Book?> LookupByIsbnAsync(string isbn, CancellationToken cancellationToken = default);
}
