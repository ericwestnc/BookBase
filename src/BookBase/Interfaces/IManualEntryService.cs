namespace BookBase.Interfaces;

public interface IManualEntryService
{
    string? TryNormalize(string rawValue);
}
