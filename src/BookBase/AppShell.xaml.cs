using BookBase.Views;

namespace BookBase;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();
        Routing.RegisterRoute(nameof(BookDetailsPage), typeof(BookDetailsPage));
        Routing.RegisterRoute(nameof(AddEditBookPage), typeof(AddEditBookPage));
        Routing.RegisterRoute(nameof(IsbnScannerPage), typeof(IsbnScannerPage));
    }
}
