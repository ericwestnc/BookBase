using System.Globalization;
using System.Text;
using System.Text.Json;
using BookBase.Interfaces;
using BookBase.Models;

namespace BookBase.Services;

public sealed class ImportExportService : IImportExportService
{
    private readonly IBookRepository _bookRepository;

    public ImportExportService(IBookRepository bookRepository)
    {
        _bookRepository = bookRepository;
    }

    public async Task ExportJsonAsync(string filePath, CancellationToken cancellationToken = default)
    {
        var books = await _bookRepository.GetAllAsync(cancellationToken);
        var json = JsonSerializer.Serialize(books, new JsonSerializerOptions { WriteIndented = true });
        await File.WriteAllTextAsync(filePath, json, cancellationToken);
    }

    public async Task ExportCsvAsync(string filePath, CancellationToken cancellationToken = default)
    {
        var books = await _bookRepository.GetAllAsync(cancellationToken);
        var sb = new StringBuilder();
        sb.AppendLine("Id,ISBN13,Title,Author,Status,Rating");
        foreach (var book in books)
        {
            sb.AppendLine($"{book.Id},{Escape(book.ISBN13)},{Escape(book.Title)},{Escape(book.Author)},{book.Status},{book.Rating.ToString(CultureInfo.InvariantCulture)}");
        }

        await File.WriteAllTextAsync(filePath, sb.ToString(), cancellationToken);
    }

    public async Task ImportJsonAsync(string filePath, CancellationToken cancellationToken = default)
    {
        var json = await File.ReadAllTextAsync(filePath, cancellationToken);
        var books = JsonSerializer.Deserialize<List<Book>>(json) ?? [];
        foreach (var book in books)
        {
            cancellationToken.ThrowIfCancellationRequested();
            book.Id = 0;
            await _bookRepository.SaveAsync(book, cancellationToken);
        }
    }

    private static string Escape(string? value) => string.IsNullOrWhiteSpace(value) ? string.Empty : $"\"{value.Replace("\"", "\"\"")}\"";
}
