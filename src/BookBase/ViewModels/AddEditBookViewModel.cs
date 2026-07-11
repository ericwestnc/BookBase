using BookBase.Interfaces;
using BookBase.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace BookBase.ViewModels;

public sealed partial class AddEditBookViewModel : BaseViewModel
{
    private readonly IBookRepository _bookRepository;
    private readonly IBookLookupService _bookLookupService;

    public AddEditBookViewModel(IBookRepository bookRepository, IBookLookupService bookLookupService)
    {
        _bookRepository = bookRepository;
        _bookLookupService = bookLookupService;
        Title = "Add / Edit Book";
        EditableBook = new Book
        {
            DateAdded = DateTimeOffset.UtcNow,
            Status = ReadingStatus.WantToRead
        };
    }

    [ObservableProperty]
    private Book editableBook;

    [RelayCommand]
    private async Task LookupByIsbnAsync(CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(EditableBook.ISBN13) && string.IsNullOrWhiteSpace(EditableBook.ISBN10))
        {
            return;
        }

        var isbn = EditableBook.ISBN13 ?? EditableBook.ISBN10!;
        var book = await _bookLookupService.LookupByIsbnAsync(isbn, cancellationToken);
        if (book is not null)
        {
            EditableBook = book;
        }
    }

    [RelayCommand]
    private async Task SaveAsync(CancellationToken cancellationToken)
    {
        await _bookRepository.SaveAsync(EditableBook, cancellationToken);
        await Shell.Current.GoToAsync("..");
    }

    [RelayCommand]
    private async Task LoadAsync(int bookId)
    {
        if (bookId <= 0)
        {
            return;
        }

        var existing = await _bookRepository.GetByIdAsync(bookId);
        if (existing is not null)
        {
            EditableBook = existing;
        }
    }
}
