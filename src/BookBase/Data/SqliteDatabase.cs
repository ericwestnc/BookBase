using BookBase.Models;
using SQLite;

namespace BookBase.Data;

public sealed class SqliteDatabase
{
    private readonly SQLiteAsyncConnection _connection;

    public SqliteDatabase()
    {
        var dbPath = Path.Combine(FileSystem.AppDataDirectory, "bookbase.db3");
        _connection = new SQLiteAsyncConnection(dbPath, SQLiteOpenFlags.Create | SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.SharedCache);
    }

    public SQLiteAsyncConnection Connection => _connection;

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        await _connection.CreateTableAsync<Book>();
        await _connection.CreateTableAsync<Author>();
        await _connection.CreateTableAsync<Series>();
        await _connection.CreateTableAsync<Publisher>();
        await _connection.CreateTableAsync<Genre>();
        await _connection.CreateTableAsync<Tag>();
        await _connection.CreateTableAsync<ReadingSession>();
        await _connection.CreateTableAsync<Collection>();
        await _connection.CreateTableAsync<Quote>();
        await _connection.CreateTableAsync<Note>();
        await _connection.CreateTableAsync<BookAuthor>();
        await _connection.CreateTableAsync<BookGenre>();
        await _connection.CreateTableAsync<BookTag>();
        await _connection.CreateTableAsync<BookCollection>();
    }
}
