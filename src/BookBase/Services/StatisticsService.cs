using BookBase.Interfaces;
using BookBase.Models;

namespace BookBase.Services;

public sealed class StatisticsService : IStatisticsService
{
    private readonly IBookRepository _bookRepository;

    public StatisticsService(IBookRepository bookRepository)
    {
        _bookRepository = bookRepository;
    }

    public async Task<DashboardStatistics> GetDashboardStatisticsAsync(CancellationToken cancellationToken = default)
    {
        var books = await _bookRepository.GetAllAsync(cancellationToken);
        var now = DateTimeOffset.UtcNow;
        var finishedThisYear = books.Count(b => b.DateFinished?.Year == now.Year);
        var ratings = books.Where(b => b.Rating > 0).Select(b => b.Rating).ToList();

        return new DashboardStatistics
        {
            CurrentlyReading = books.Count(b => b.Status == ReadingStatus.Reading),
            BooksFinishedThisYear = finishedThisYear,
            BooksOwned = books.Count(b => b.Owned),
            WishlistCount = books.Count(b => b.Wishlist),
            ReadingStreak = 0,
            PagesRead = books.Where(b => b.Status == ReadingStatus.Finished).Sum(b => b.TotalPages),
            AverageRating = ratings.Count == 0 ? 0 : ratings.Average()
        };
    }
}
