using BookBase.Interfaces;
using BookBase.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace BookBase.ViewModels;

public sealed partial class BookDetailsViewModel : BaseViewModel
{
    private readonly IBookRepository _bookRepository;

    public BookDetailsViewModel(IBookRepository bookRepository)
    {
        _bookRepository = bookRepository;
        Title = "Book Details";
    }

    [ObservableProperty]
    private Book? book;

    public double Progress => Book is null || Book.TotalPages <= 0 ? 0 : Math.Clamp((double)Book.CurrentPage / Book.TotalPages, 0, 1);

    [RelayCommand]
    private async Task LoadAsync(int bookId)
    {
        var loadedBook = await _bookRepository.GetByIdAsync(bookId);
        Book = loadedBook ?? new Book { Title = "Book not found" };
        OnPropertyChanged(nameof(Progress));
    }
}
