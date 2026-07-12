namespace BookBase.Interfaces;

public interface ISettingsService
{
    AppTheme Theme { get; set; }
    string PreferredSortOrder { get; set; }
    int PreferredGridSize { get; set; }
    string AccentColor { get; set; }
    int ReadingGoal { get; set; }
}
