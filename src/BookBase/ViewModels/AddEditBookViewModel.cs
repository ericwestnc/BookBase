using BookBase.Interfaces;
using BookBase.Models;
using BookBase.Utilities;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace BookBase.ViewModels;

public sealed partial class AddEditBookViewModel : BaseViewModel
{
    private readonly IBookRepository _bookRepository;
    private readonly IBookLookupService _bookLookupService;
    private readonly Func<string, string?> _manualIsbnNormalizer;

    public AddEditBookViewModel(
        IBookRepository bookRepository,
        IBookLookupService bookLookupService,
        IManualEntryService? manualEntryService = null)
    {
        _bookRepository = bookRepository;
        _bookLookupService = bookLookupService;
        _manualIsbnNormalizer = manualEntryService is null
            ? DefaultNormalizeManualIsbn
            : manualEntryService.TryNormalize;
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
        var normalized = _manualIsbnNormalizer(isbn);
        if (string.IsNullOrWhiteSpace(normalized))
        {
            return;
        }

        // Clone the current book so the UI refreshes (Book doesn't implement
        // INotifyPropertyChanged; only replacing the EditableBook reference triggers
        // the observable update).
        var updated = EditableBook.Clone();
        if (normalized.Length == 13)
            updated.ISBN13 = normalized;
        else if (normalized.Length == 10)
            updated.ISBN10 = normalized;
        EditableBook = updated;

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
        var normalized = _manualIsbnNormalizer(isbn);
        if (string.IsNullOrWhiteSpace(normalized))
        {
            return;
        }

        if (normalized.Length == 13)
        {
            EditableBook.ISBN13 = normalized;
        }
        else
        {
            EditableBook.ISBN10 = normalized;
        }

        var book = await _bookLookupService.LookupByIsbnAsync(normalized, cancellationToken);
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

    private static string? DefaultNormalizeManualIsbn(string rawValue)
    {
        var normalized = IsbnNormalizer.Normalize(rawValue);
        return IsbnValidator.IsValid(normalized) ? normalized : null;
    }
}
