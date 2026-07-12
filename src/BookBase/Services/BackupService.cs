using BookBase.Interfaces;

namespace BookBase.Services;

public sealed class BackupService : IBackupService
{
    public async Task BackupAsync(string backupFilePath, CancellationToken cancellationToken = default)
    {
        var dbPath = Path.Combine(FileSystem.AppDataDirectory, "bookbase.db3");
        await using var source = File.Open(dbPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        await using var destination = File.Open(backupFilePath, FileMode.Create, FileAccess.Write, FileShare.None);
        await source.CopyToAsync(destination, cancellationToken);
    }

    public async Task RestoreAsync(string backupFilePath, CancellationToken cancellationToken = default)
    {
        var dbPath = Path.Combine(FileSystem.AppDataDirectory, "bookbase.db3");
        await using var source = File.Open(backupFilePath, FileMode.Open, FileAccess.Read, FileShare.Read);
        await using var destination = File.Open(dbPath, FileMode.Create, FileAccess.Write, FileShare.None);
        await source.CopyToAsync(destination, cancellationToken);
    }
}
