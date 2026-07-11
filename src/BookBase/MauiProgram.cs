using BookBase.Converters;
using BookBase.Data;
using BookBase.Interfaces;
using BookBase.Repositories;
using BookBase.Services;
using BookBase.ViewModels;
using CommunityToolkit.Maui;

namespace BookBase;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseMauiCommunityToolkit()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        builder.Services.AddSingleton<SqliteDatabase>();
        builder.Services.AddSingleton<IBookRepository, BookRepository>();

        builder.Services.AddSingleton<IBookLookupService, BookLookupService>();
        builder.Services.AddSingleton<IStatisticsService, StatisticsService>();
        builder.Services.AddSingleton<IReadingProgressService, ReadingProgressService>();
        builder.Services.AddSingleton<IImportExportService, ImportExportService>();
        builder.Services.AddSingleton<IBackupService, BackupService>();
        builder.Services.AddSingleton<ISettingsService, SettingsService>();

        builder.Services.AddHttpClient(nameof(BookLookupService));

        builder.Services.AddTransient<DashboardViewModel>();
        builder.Services.AddTransient<LibraryViewModel>();
        builder.Services.AddTransient<BookDetailsViewModel>();
        builder.Services.AddTransient<AddEditBookViewModel>();

        builder.Services.AddTransient<Views.DashboardPage>();
        builder.Services.AddTransient<Views.LibraryPage>();
        builder.Services.AddTransient<Views.BookDetailsPage>();
        builder.Services.AddTransient<Views.AddEditBookPage>();

        builder.Services.AddSingleton<BoolInverseConverter>();

        var app = builder.Build();

        _ = InitializeDatabaseAsync(app.Services);

        return app;
    }

    private static async Task InitializeDatabaseAsync(IServiceProvider serviceProvider)
    {
        var database = serviceProvider.GetRequiredService<SqliteDatabase>();
        await database.InitializeAsync();
    }
}
