using BookBase.Data;
using BookBase.Interfaces;
using BookBase.Models;
using BookBase.Utilities;

namespace BookBase.Repositories;

public sealed class BookRepository : IBookRepository
{
    private readonly SqliteDatabase _database;

    public BookRepository(SqliteDatabase database)
    {
        _database = database;
    }

    public async Task<IReadOnlyList<Book>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return await _database.Connection.Table<Book>().OrderByDescending(b => b.DateAdded).ToListAsync();
    }

    public async Task<Book?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return await _database.Connection.Table<Book>().FirstOrDefaultAsync(b => b.Id == id);
    }

    public async Task<int> SaveAsync(Book entity, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        entity.ISBN10 = NormalizeForStorage(entity.ISBN10);
        entity.ISBN13 = NormalizeForStorage(entity.ISBN13);

        return entity.Id == 0
            ? await _database.Connection.InsertAsync(entity)
            : await _database.Connection.UpdateAsync(entity);
    }

    public async Task<int> DeleteAsync(Book entity, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return await _database.Connection.DeleteAsync(entity);
    }

    public async Task<IReadOnlyList<Book>> SearchAsync(string query, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (string.IsNullOrWhiteSpace(query))
        {
            return await GetAllAsync(cancellationToken);
        }

        var q = query.Trim().ToLowerInvariant();
        return await _database.Connection.Table<Book>()
            .Where(b => b.Title.ToLower().Contains(q)
                || (b.Author ?? string.Empty).ToLower().Contains(q)
                || (b.ISBN13 ?? string.Empty).ToLower().Contains(q)
                || (b.ISBN10 ?? string.Empty).ToLower().Contains(q))
            .ToListAsync();
    }

    public async Task<Book?> GetByIsbnAsync(string isbn, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var normalized = IsbnNormalizer.Normalize(isbn);
        if (string.IsNullOrWhiteSpace(normalized))
        {
            return null;
        }

        var exactMatch = await _database.Connection.Table<Book>()
            .FirstOrDefaultAsync(b => b.ISBN10 == normalized || b.ISBN13 == normalized);
        if (exactMatch is not null)
        {
            return exactMatch;
        }

        var books = await _database.Connection.Table<Book>()
            .Where(b =>
                (b.ISBN10 != null && (b.ISBN10.Contains("-") || b.ISBN10.Contains(" ")))
                || (b.ISBN13 != null && (b.ISBN13.Contains("-") || b.ISBN13.Contains(" "))))
            .ToListAsync();

        return books.FirstOrDefault(b =>
            string.Equals(IsbnNormalizer.Normalize(b.ISBN10), normalized, StringComparison.Ordinal)
            || string.Equals(IsbnNormalizer.Normalize(b.ISBN13), normalized, StringComparison.Ordinal));
    }

    public async Task<IReadOnlyList<Book>> GetByStatusAsync(ReadingStatus status, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return await _database.Connection.Table<Book>().Where(b => b.Status == status).ToListAsync();
    }

    public async Task<IReadOnlyList<Book>> GetRecentlyFinishedAsync(int count, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return await _database.Connection.Table<Book>()
            .Where(b => b.Status == ReadingStatus.Finished)
            .OrderByDescending(b => b.DateFinished)
            .Take(count)
            .ToListAsync();
    }

    public async Task<IReadOnlyList<Book>> GetNewestAsync(int count, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return await _database.Connection.Table<Book>().OrderByDescending(b => b.DateAdded).Take(count).ToListAsync();
    }

    private static string? NormalizeForStorage(string? isbn)
    {
        var normalized = IsbnNormalizer.Normalize(isbn);
        return string.IsNullOrWhiteSpace(normalized) ? null : normalized;
    }
}
