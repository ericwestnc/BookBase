namespace BookBase.Models;

public sealed class DashboardStatistics
{
    public int CurrentlyReading { get; set; }
    public int BooksFinishedThisYear { get; set; }
    public int BooksOwned { get; set; }
    public int WishlistCount { get; set; }
    public int ReadingStreak { get; set; }
    public int PagesRead { get; set; }
    public double AverageRating { get; set; }
}
