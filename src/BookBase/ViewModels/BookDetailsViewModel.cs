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

    [RelayCommand]
    private async Task LoadAsync(int bookId)
    {
        Book = await _bookRepository.GetByIdAsync(bookId);
    }
}
