using System.Text.Json;
using BookBase.Interfaces;
using BookBase.Models;
using BookBase.Utilities;

namespace BookBase.Services;

public sealed class BookLookupService : IBookLookupService
{
    private readonly IBookRepository _bookRepository;
    private readonly IHttpClientFactory _httpClientFactory;

    public BookLookupService(IBookRepository bookRepository, IHttpClientFactory httpClientFactory)
    {
        _bookRepository = bookRepository;
        _httpClientFactory = httpClientFactory;
    }

    public async Task<Book?> LookupByIsbnAsync(string isbn, CancellationToken cancellationToken = default)
    {
        var normalized = IsbnNormalizer.Normalize(isbn);
        if (string.IsNullOrWhiteSpace(normalized))
        {
            return null;
        }

        var existing = await _bookRepository.GetByIsbnAsync(normalized, cancellationToken);
        if (existing is not null)
        {
            return existing;
        }

        var fromOpenLibrary = await QueryOpenLibraryAsync(normalized, cancellationToken);
        if (fromOpenLibrary is not null)
        {
            return fromOpenLibrary;
        }

        var fromGoogleBooks = await QueryGoogleBooksAsync(normalized, cancellationToken);
        if (fromGoogleBooks is not null)
        {
            return fromGoogleBooks;
        }

        return null;
    }

    private async Task<Book?> QueryOpenLibraryAsync(string isbn, CancellationToken cancellationToken)
    {
        var client = _httpClientFactory.CreateClient(nameof(BookLookupService));
        using var response = await client.GetAsync($"https://openlibrary.org/isbn/{isbn}.json", cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var json = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);
        var root = json.RootElement;

        return new Book
        {
            Id = 0,
            ISBN10 = isbn.Length == 10 ? isbn : null,
            ISBN13 = isbn.Length == 13 ? isbn : null,
            Title = root.TryGetProperty("title", out var title) ? title.GetString() ?? "Untitled" : "Untitled",
            Subtitle = root.TryGetProperty("subtitle", out var subtitle) ? subtitle.GetString() : null,
            PublishedDate = DateTimeOffset.TryParse(root.TryGetProperty("publish_date", out var publishDate) ? publishDate.GetString() : null, out var dt) ? dt : null,
            PageCount = root.TryGetProperty("number_of_pages", out var pages) && pages.TryGetInt32(out var numberOfPages) ? numberOfPages : null,
            Status = ReadingStatus.WantToRead,
            DateAdded = DateTimeOffset.UtcNow,
            Owned = false,
            Wishlist = true,
            CoverUrl = $"https://covers.openlibrary.org/b/isbn/{isbn}-L.jpg"
        };
    }

    private async Task<Book?> QueryGoogleBooksAsync(string isbn, CancellationToken cancellationToken)
    {
        var client = _httpClientFactory.CreateClient(nameof(BookLookupService));
        using var response = await client.GetAsync($"https://www.googleapis.com/books/v1/volumes?q=isbn:{isbn}", cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var json = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);
        var root = json.RootElement;
        if (!root.TryGetProperty("items", out var items) || items.GetArrayLength() == 0)
        {
            return null;
        }

        var volumeInfo = items[0].GetProperty("volumeInfo");
        string? author = null;
        if (volumeInfo.TryGetProperty("authors", out var authors) && authors.GetArrayLength() > 0)
        {
            author = authors[0].GetString();
        }

        return new Book
        {
            Id = 0,
            ISBN10 = isbn.Length == 10 ? isbn : null,
            ISBN13 = isbn.Length == 13 ? isbn : null,
            Title = volumeInfo.TryGetProperty("title", out var title) ? title.GetString() ?? "Untitled" : "Untitled",
            Subtitle = volumeInfo.TryGetProperty("subtitle", out var subtitle) ? subtitle.GetString() : null,
            Description = volumeInfo.TryGetProperty("description", out var desc) ? desc.GetString() : null,
            Author = author,
            Publisher = volumeInfo.TryGetProperty("publisher", out var publisher) ? publisher.GetString() : null,
            PageCount = volumeInfo.TryGetProperty("pageCount", out var pageCount) && pageCount.TryGetInt32(out var pages) ? pages : null,
            Language = volumeInfo.TryGetProperty("language", out var language) ? language.GetString() : null,
            Status = ReadingStatus.WantToRead,
            DateAdded = DateTimeOffset.UtcNow,
            Owned = false,
            Wishlist = true,
            CoverUrl = volumeInfo.TryGetProperty("imageLinks", out var imageLinks) && imageLinks.TryGetProperty("thumbnail", out var thumb)
                ? thumb.GetString()
                : null
        };
    }
}
