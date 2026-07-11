namespace BookBase.Interfaces;

public interface IBackupService
{
    Task BackupAsync(string backupFilePath, CancellationToken cancellationToken = default);
    Task RestoreAsync(string backupFilePath, CancellationToken cancellationToken = default);
}
