using BookBase.Interfaces;

namespace BookBase.Services;

public sealed class BackupService : IBackupService
{
    public Task BackupAsync(string backupFilePath, CancellationToken cancellationToken = default)
    {
        var dbPath = Path.Combine(FileSystem.AppDataDirectory, "bookbase.db3");
        File.Copy(dbPath, backupFilePath, overwrite: true);
        return Task.CompletedTask;
    }

    public Task RestoreAsync(string backupFilePath, CancellationToken cancellationToken = default)
    {
        var dbPath = Path.Combine(FileSystem.AppDataDirectory, "bookbase.db3");
        File.Copy(backupFilePath, dbPath, overwrite: true);
        return Task.CompletedTask;
    }
}
