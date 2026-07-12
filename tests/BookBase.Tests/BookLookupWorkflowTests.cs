using System.Net;
using System.Reflection;
using System.Text;
using BookBase.Data;
using BookBase.Interfaces;
using BookBase.Models;
using BookBase.Repositories;
using BookBase.Services;
using BookBase.ViewModels;

namespace BookBase.Tests;

public sealed class BookLookupWorkflowTests
{
    private const string NormalizedIsbn13 = "9781402894626";
    private const string HyphenatedIsbn13 = "978-1-4028-9462-6";
    private const string SpacedIsbn13 = "978 1 4028 9462 6";
    private const string MixedIsbn13 = "978-1 4028-9462-6";

    [Fact]
    public async Task OpenLibraryLookup_ReturnsMetadata_WithoutSaving()
    {
        var repository = new TrackingBookRepository();
        var service = new BookLookupService(
            repository,
            new StubHttpClientFactory(_ => JsonResponse("""
                {
                  "title": "Open Library Title",
                  "subtitle": "Open Library Subtitle",
                  "publish_date": "2024-01-15",
                  "number_of_pages": 321
                }
                """)));

        var book = await service.LookupByIsbnAsync(HyphenatedIsbn13);

        Assert.NotNull(book);
        Assert.Equal(0, book!.Id);
        Assert.Equal(NormalizedIsbn13, book.ISBN13);
        Assert.Equal("Open Library Title", book.Title);
        Assert.Equal(0, repository.SaveCallCount);
    }

    [Fact]
    public async Task GoogleBooksFallback_ReturnsMetadata_WithoutSaving()
    {
        var repository = new TrackingBookRepository();
        var service = new BookLookupService(
            repository,
            new StubHttpClientFactory(request =>
            {
                if (request.RequestUri!.Host.Contains("openlibrary.org", StringComparison.Ordinal))
                {
                    return new HttpResponseMessage(HttpStatusCode.NotFound);
                }

                return JsonResponse("""
                    {
                      "items": [
                        {
                          "volumeInfo": {
                            "title": "Google Books Title",
                            "subtitle": "Google Books Subtitle",
                            "description": "Google description",
                            "authors": ["Google Author"],
                            "publisher": "Google Publisher",
                            "pageCount": 222,
                            "language": "en",
                            "imageLinks": {
                              "thumbnail": "https://example.test/google.jpg"
                            }
                          }
                        }
                      ]
                    }
                    """);
            }));

        var book = await service.LookupByIsbnAsync(SpacedIsbn13);

        Assert.NotNull(book);
        Assert.Equal(0, book!.Id);
        Assert.Equal(NormalizedIsbn13, book.ISBN13);
        Assert.Equal("Google Books Title", book.Title);
        Assert.Equal(0, repository.SaveCallCount);
    }

    [Fact]
    public async Task ExistingLocalIsbn_ReturnsExistingBook_AndPreservesId()
    {
        await using var harness = await RepositoryHarness.CreateAsync();
        var existing = new Book
        {
            Title = "Existing",
            ISBN13 = NormalizedIsbn13,
            Owned = true,
            Wishlist = false
        };

        await harness.Repository.SaveAsync(existing);

        var service = new BookLookupService(
            harness.Repository,
            new StubHttpClientFactory(_ => throw new InvalidOperationException("HTTP should not be used for local matches.")));

        var book = await service.LookupByIsbnAsync(MixedIsbn13);

        Assert.NotNull(book);
        Assert.Equal(existing.Id, book!.Id);
        Assert.Equal("Existing", book.Title);
    }

    [Fact]
    public async Task SavingNewlyLookedUpBook_InsertsExactlyOneRecord()
    {
        await using var harness = await RepositoryHarness.CreateAsync();
        var service = new BookLookupService(
            harness.Repository,
            new StubHttpClientFactory(_ => JsonResponse("""
                {
                  "title": "Lookup Result",
                  "publish_date": "2025-02-20",
                  "number_of_pages": 123
                }
                """)));

        var viewModel = new AddEditBookViewModel(harness.Repository, service)
        {
            EditableBook = new Book
            {
                ISBN13 = HyphenatedIsbn13,
                Owned = false,
                Wishlist = true
            }
        };

        await InvokeAsync(viewModel, "LookupByIsbnAsync");
        Assert.Equal(0, viewModel.EditableBook.Id);

        await InvokeAsync(viewModel, "SaveAsync");

        var books = await harness.Repository.GetAllAsync();

        Assert.Single(books);
        Assert.Equal("Lookup Result", books[0].Title);
        Assert.Equal(NormalizedIsbn13, books[0].ISBN13);
    }

    [Fact]
    public async Task LookingUpAndSavingExistingIsbn_DoesNotCreateDuplicate()
    {
        await using var harness = await RepositoryHarness.CreateAsync();
        var existing = new Book
        {
            Title = "Already Stored",
            ISBN13 = NormalizedIsbn13,
            Owned = true,
            Wishlist = false,
            Status = ReadingStatus.Finished
        };

        await harness.Repository.SaveAsync(existing);

        var service = new BookLookupService(
            harness.Repository,
            new StubHttpClientFactory(_ => throw new InvalidOperationException("HTTP should not be used for local matches.")));
        var viewModel = new AddEditBookViewModel(harness.Repository, service)
        {
            EditableBook = new Book
            {
                ISBN13 = MixedIsbn13
            }
        };

        await InvokeAsync(viewModel, "LookupByIsbnAsync");
        await InvokeAsync(viewModel, "SaveAsync");

        var books = await harness.Repository.GetAllAsync();

        Assert.Single(books);
        Assert.Equal(existing.Id, viewModel.EditableBook.Id);
        Assert.Equal(existing.Id, books[0].Id);
        Assert.Equal("Already Stored", books[0].Title);
        Assert.Equal(ReadingStatus.Finished, books[0].Status);
    }

    [Fact]
    public async Task RepositoryLookup_NormalizesSpacesAndHyphens()
    {
        await using var harness = await RepositoryHarness.CreateAsync();
        await harness.Database.Connection.InsertAsync(new Book
        {
            Title = "Normalized Match",
            ISBN13 = HyphenatedIsbn13
        });

        var book = await harness.Repository.GetByIsbnAsync("978 1402894626");

        Assert.NotNull(book);
        Assert.Equal("Normalized Match", book!.Title);
    }

    private static HttpResponseMessage JsonResponse(string json) => new(HttpStatusCode.OK)
    {
        Content = new StringContent(json, Encoding.UTF8, "application/json")
    };

    private static async Task InvokeAsync(object target, string methodName)
    {
        var method = target.GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.NotNull(method);

        var result = method!.Invoke(target, [CancellationToken.None]);
        await Assert.IsAssignableFrom<Task>(result);
    }

    private sealed class TrackingBookRepository : IBookRepository
    {
        private readonly List<Book> _books = [];

        public int SaveCallCount { get; private set; }

        public Task<IReadOnlyList<Book>> GetAllAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<Book>>(_books.ToList());

        public Task<Book?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
            Task.FromResult(_books.FirstOrDefault(book => book.Id == id));

        public Task<int> SaveAsync(Book entity, CancellationToken cancellationToken = default)
        {
            SaveCallCount++;
            return Task.FromResult(1);
        }

        public Task<int> DeleteAsync(Book entity, CancellationToken cancellationToken = default) =>
            Task.FromResult(1);

        public Task<IReadOnlyList<Book>> SearchAsync(string query, CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<Book>>(_books.ToList());

        public Task<Book?> GetByIsbnAsync(string isbn, CancellationToken cancellationToken = default) =>
            Task.FromResult<Book?>(null);

        public Task<IReadOnlyList<Book>> GetByStatusAsync(ReadingStatus status, CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<Book>>(_books.Where(book => book.Status == status).ToList());

        public Task<IReadOnlyList<Book>> GetRecentlyFinishedAsync(int count, CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<Book>>(_books.Take(count).ToList());

        public Task<IReadOnlyList<Book>> GetNewestAsync(int count, CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<Book>>(_books.Take(count).ToList());
    }

    private sealed class StubHttpClientFactory(Func<HttpRequestMessage, HttpResponseMessage> responseFactory) : IHttpClientFactory
    {
        public HttpClient CreateClient(string name) => new(new StubHttpMessageHandler(responseFactory))
        {
            BaseAddress = new Uri("https://example.test/")
        };
    }

    private sealed class StubHttpMessageHandler(Func<HttpRequestMessage, HttpResponseMessage> responseFactory) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) =>
            Task.FromResult(responseFactory(request));
    }

    private sealed class RepositoryHarness : IAsyncDisposable
    {
        private RepositoryHarness(string databasePath, SqliteDatabase database, BookRepository repository)
        {
            DatabasePath = databasePath;
            Database = database;
            Repository = repository;
        }

        public string DatabasePath { get; }

        public SqliteDatabase Database { get; }

        public BookRepository Repository { get; }

        public static async Task<RepositoryHarness> CreateAsync()
        {
            Directory.CreateDirectory(FileSystem.AppDataDirectory);

            var databasePath = Path.Combine(FileSystem.AppDataDirectory, $"{Guid.NewGuid():N}.db3");
            var database = new SqliteDatabase(databasePath);
            await database.Connection.CreateTableAsync<Book>();
            return new RepositoryHarness(databasePath, database, new BookRepository(database));
        }

        public ValueTask DisposeAsync()
        {
            if (File.Exists(DatabasePath))
            {
                File.Delete(DatabasePath);
            }

            return ValueTask.CompletedTask;
        }
    }
}
