namespace BookBase.Interfaces;

public interface IReadingProgressService
{
    Task UpdateProgressAsync(int bookId, int currentPage, CancellationToken cancellationToken = default);
}
