using BookBase.Models;

namespace BookBase.Interfaces;

public interface IBookRepository : IRepository<Book>
{
    Task<IReadOnlyList<Book>> SearchAsync(string query, CancellationToken cancellationToken = default);
    Task<Book?> GetByIsbnAsync(string isbn, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Book>> GetByStatusAsync(ReadingStatus status, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Book>> GetRecentlyFinishedAsync(int count, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Book>> GetNewestAsync(int count, CancellationToken cancellationToken = default);
}
