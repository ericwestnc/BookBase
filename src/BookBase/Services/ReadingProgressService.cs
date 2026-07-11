using BookBase.Interfaces;
using BookBase.Models;

namespace BookBase.Services;

public sealed class ReadingProgressService : IReadingProgressService
{
    private readonly IBookRepository _bookRepository;
    private readonly Data.SqliteDatabase _database;

    public ReadingProgressService(IBookRepository bookRepository, Data.SqliteDatabase database)
    {
        _bookRepository = bookRepository;
        _database = database;
    }

    public async Task UpdateProgressAsync(int bookId, int currentPage, CancellationToken cancellationToken = default)
    {
        var book = await _bookRepository.GetByIdAsync(bookId, cancellationToken) ?? throw new InvalidOperationException($"Book with ID {bookId} not found.");
        var previousPage = book.CurrentPage;
        book.CurrentPage = Math.Max(0, currentPage);

        if (book.CurrentPage > 0 && book.DateStarted is null)
        {
            book.DateStarted = DateTimeOffset.UtcNow;
        }

        if (book.TotalPages > 0 && book.CurrentPage >= book.TotalPages)
        {
            book.Status = ReadingStatus.Finished;
            book.DateFinished = DateTimeOffset.UtcNow;
        }
        else if (book.CurrentPage > 0)
        {
            book.Status = ReadingStatus.Reading;
        }

        await _bookRepository.SaveAsync(book, cancellationToken);

        await _database.Connection.InsertAsync(new ReadingSession
        {
            BookId = bookId,
            StartTime = DateTimeOffset.UtcNow,
            EndTime = DateTimeOffset.UtcNow,
            PagesRead = Math.Max(0, book.CurrentPage - previousPage)
        });
    }
}
