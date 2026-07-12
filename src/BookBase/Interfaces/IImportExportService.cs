namespace BookBase.Interfaces;

public interface IImportExportService
{
    Task ExportJsonAsync(string filePath, CancellationToken cancellationToken = default);
    Task ExportCsvAsync(string filePath, CancellationToken cancellationToken = default);
    Task ImportJsonAsync(string filePath, CancellationToken cancellationToken = default);
}
