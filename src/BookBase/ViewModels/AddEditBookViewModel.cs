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
        EditableBook = CreateDefaultBook();
    }

    [ObservableProperty]
    private Book editableBook;

    /// <summary>
    /// Called by <see cref="Views.AddEditBookPage"/> when the page receives
    /// a <c>scannedIsbn</c> Shell query parameter from the scanner page.
    /// Sets the appropriate ISBN field and triggers an automatic lookup.
    /// </summary>
    public async Task ApplyScannedIsbnAsync(string isbn, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(isbn))
        {
            return;
        }

        EditableBook = new Book
        {
            Id = EditableBook.Id,
            ISBN10 = isbn.Length == 10 ? isbn : EditableBook.ISBN10,
            ISBN13 = isbn.Length == 13 ? isbn : EditableBook.ISBN13,
            Title = EditableBook.Title,
            Subtitle = EditableBook.Subtitle,
            Description = EditableBook.Description,
            Author = EditableBook.Author,
            Publisher = EditableBook.Publisher,
            PublishedDate = EditableBook.PublishedDate,
            PageCount = EditableBook.PageCount,
            Language = EditableBook.Language,
            CoverUrl = EditableBook.CoverUrl,
            CoverImage = EditableBook.CoverImage,
            Format = EditableBook.Format,
            Status = EditableBook.Status,
            Rating = EditableBook.Rating,
            PersonalNotes = EditableBook.PersonalNotes,
            CurrentPage = EditableBook.CurrentPage,
            TotalPages = EditableBook.TotalPages,
            DateAdded = EditableBook.DateAdded,
            DateStarted = EditableBook.DateStarted,
            DateFinished = EditableBook.DateFinished,
            Favorite = EditableBook.Favorite,
            Owned = EditableBook.Owned,
            Wishlist = EditableBook.Wishlist,
            PricePaid = EditableBook.PricePaid,
            LocationOnShelf = EditableBook.LocationOnShelf
        };

        await LookupByIsbnAsync(cancellationToken);
    }

    [RelayCommand]
    private static async Task ScanIsbnAsync()
    {
        await Shell.Current.GoToAsync("IsbnScannerPage");
    }

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
            if (book.Id > 0)
            {
                EditableBook = book;
                return;
            }

            EditableBook = new Book
            {
                Id = EditableBook.Id,
                ISBN10 = book.ISBN10 ?? EditableBook.ISBN10,
                ISBN13 = book.ISBN13 ?? EditableBook.ISBN13,
                Title = string.IsNullOrWhiteSpace(book.Title) ? EditableBook.Title : book.Title,
                Subtitle = book.Subtitle ?? EditableBook.Subtitle,
                Description = book.Description ?? EditableBook.Description,
                Author = book.Author ?? EditableBook.Author,
                Publisher = book.Publisher ?? EditableBook.Publisher,
                PublishedDate = book.PublishedDate ?? EditableBook.PublishedDate,
                PageCount = book.PageCount ?? EditableBook.PageCount,
                Language = book.Language ?? EditableBook.Language,
                CoverUrl = book.CoverUrl ?? EditableBook.CoverUrl,
                CoverImage = EditableBook.CoverImage,
                Format = EditableBook.Format,
                Status = EditableBook.Status,
                Rating = EditableBook.Rating,
                PersonalNotes = EditableBook.PersonalNotes,
                CurrentPage = EditableBook.CurrentPage,
                TotalPages = EditableBook.TotalPages,
                DateAdded = EditableBook.DateAdded,
                DateStarted = EditableBook.DateStarted,
                DateFinished = EditableBook.DateFinished,
                Favorite = EditableBook.Favorite,
                Owned = EditableBook.Owned,
                Wishlist = EditableBook.Wishlist,
                PricePaid = EditableBook.PricePaid,
                LocationOnShelf = EditableBook.LocationOnShelf
            };
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
            EditableBook = CreateDefaultBook();
            return;
        }

        var existing = await _bookRepository.GetByIdAsync(bookId);
        if (existing is not null)
        {
            EditableBook = existing;
        }
    }

    private static Book CreateDefaultBook() => new()
    {
        DateAdded = DateTimeOffset.UtcNow,
        Status = ReadingStatus.WantToRead
    };
}
