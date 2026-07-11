using BookBase.Interfaces;

namespace BookBase.Services;

public sealed class SettingsService : ISettingsService
{
    public AppTheme Theme
    {
        get => (AppTheme)Preferences.Default.Get(nameof(Theme), (int)AppTheme.Unspecified);
        set => Preferences.Default.Set(nameof(Theme), (int)value);
    }

    public string PreferredSortOrder
    {
        get => Preferences.Default.Get(nameof(PreferredSortOrder), "TitleAsc");
        set => Preferences.Default.Set(nameof(PreferredSortOrder), value);
    }

    public int PreferredGridSize
    {
        get => Preferences.Default.Get(nameof(PreferredGridSize), 2);
        set => Preferences.Default.Set(nameof(PreferredGridSize), value);
    }

    public string AccentColor
    {
        get => Preferences.Default.Get(nameof(AccentColor), "#6750A4");
        set => Preferences.Default.Set(nameof(AccentColor), value);
    }

    public int ReadingGoal
    {
        get => Preferences.Default.Get(nameof(ReadingGoal), 12);
        set => Preferences.Default.Set(nameof(ReadingGoal), value);
    }
}
