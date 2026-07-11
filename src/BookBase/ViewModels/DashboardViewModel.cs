using BookBase.Interfaces;
using BookBase.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace BookBase.ViewModels;

public sealed partial class DashboardViewModel : BaseViewModel
{
    private readonly IStatisticsService _statisticsService;
    private readonly IBookRepository _bookRepository;

    public DashboardViewModel(IStatisticsService statisticsService, IBookRepository bookRepository)
    {
        _statisticsService = statisticsService;
        _bookRepository = bookRepository;
        Title = "Dashboard";
    }

    [ObservableProperty]
    private DashboardStatistics statistics = new();

    public ObservableCollection<Book> NewestBooks { get; } = [];
    public ObservableCollection<Book> RecentlyFinished { get; } = [];

    [RelayCommand]
    private async Task LoadAsync(CancellationToken cancellationToken)
    {
        if (IsBusy)
        {
            return;
        }

        try
        {
            IsBusy = true;
            Statistics = await _statisticsService.GetDashboardStatisticsAsync(cancellationToken);

            NewestBooks.Clear();
            RecentlyFinished.Clear();
            foreach (var book in await _bookRepository.GetNewestAsync(8, cancellationToken))
            {
                NewestBooks.Add(book);
            }

            foreach (var book in await _bookRepository.GetRecentlyFinishedAsync(8, cancellationToken))
            {
                RecentlyFinished.Add(book);
            }
        }
        finally
        {
            IsBusy = false;
        }
    }
}
