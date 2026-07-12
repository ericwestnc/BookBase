using BookBase.Converters;
using BookBase.Data;
using BookBase.Interfaces;
using BookBase.Repositories;
using BookBase.Services;
using BookBase.ViewModels;
using CommunityToolkit.Maui;
using ZXing.Net.Maui.Controls;

namespace BookBase;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseMauiCommunityToolkit()
            .UseBarcodeReader()
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
        builder.Services.AddSingleton<IBarcodeRecognitionService, BarcodeRecognitionService>();
        builder.Services.AddSingleton<IPrintedIsbnRecognitionService, PrintedIsbnRecognitionService>();
        builder.Services.AddSingleton<IManualEntryService, ManualEntryService>();

#if ANDROID
        builder.Services.AddSingleton<IIsbnTextRecognitionService,
            BookBase.Platforms.Android.AndroidIsbnTextRecognitionService>();
#else
        builder.Services.AddSingleton<IIsbnTextRecognitionService,
            UnsupportedIsbnTextRecognitionService>();
#endif

        builder.Services.AddHttpClient(nameof(BookLookupService));

        builder.Services.AddTransient<DashboardViewModel>();
        builder.Services.AddTransient<LibraryViewModel>();
        builder.Services.AddTransient<BookDetailsViewModel>();
        builder.Services.AddTransient<AddEditBookViewModel>();
        builder.Services.AddTransient<IsbnScannerViewModel>();

        builder.Services.AddTransient<Views.DashboardPage>();
        builder.Services.AddTransient<Views.LibraryPage>();
        builder.Services.AddTransient<Views.BookDetailsPage>();
        builder.Services.AddTransient<Views.AddEditBookPage>();
        builder.Services.AddTransient<Views.IsbnScannerPage>();

        builder.Services.AddSingleton<BoolInverseConverter>();

        var app = builder.Build();

        InitializeDatabase(app.Services);

        return app;
    }

    private static void InitializeDatabase(IServiceProvider serviceProvider)
    {
        var database = serviceProvider.GetRequiredService<SqliteDatabase>();
        database.InitializeAsync().GetAwaiter().GetResult();
    }
}
