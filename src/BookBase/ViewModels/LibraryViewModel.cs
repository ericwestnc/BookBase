using BookBase.Interfaces;
using BookBase.Models;
using BookBase.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace BookBase.ViewModels;

public sealed partial class LibraryViewModel : BaseViewModel
{
    private readonly IBookRepository _bookRepository;

    public LibraryViewModel(IBookRepository bookRepository)
    {
        _bookRepository = bookRepository;
        Title = "Library";
    }

    [ObservableProperty]
    private string searchText = string.Empty;

    [ObservableProperty]
    private bool gridView = true;

    public ObservableCollection<Book> Books { get; } = [];

    [RelayCommand]
    private async Task LoadAsync(CancellationToken cancellationToken)
    {
        await RefreshAsync(cancellationToken);
    }

    [RelayCommand]
    private async Task SearchAsync(CancellationToken cancellationToken)
    {
        await RefreshAsync(cancellationToken);
    }

    [RelayCommand]
    private void ToggleView()
    {
        GridView = !GridView;
    }

    [RelayCommand]
    private Task EditAsync(Book book)
    {
        return Shell.Current.GoToAsync($"{nameof(AddEditBookPage)}?bookId={book.Id}");
    }

    private async Task RefreshAsync(CancellationToken cancellationToken)
    {
        var books = await _bookRepository.SearchAsync(SearchText, cancellationToken);
        Books.Clear();
        foreach (var book in books)
        {
            Books.Add(book);
        }
    }
}
