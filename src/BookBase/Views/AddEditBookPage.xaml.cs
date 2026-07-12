using BookBase.Helpers;
using BookBase.ViewModels;

namespace BookBase.Views;

[QueryProperty(nameof(BookId), "bookId")]
[QueryProperty(nameof(ScannedIsbn), "scannedIsbn")]
public partial class AddEditBookPage : ContentPage
{
    private int _bookId;

    public AddEditBookPage()
    {
        InitializeComponent();
        BindingContext = ServiceHelper.GetService<AddEditBookViewModel>();
    }

    public int BookId
    {
        get => _bookId;
        set
        {
            _bookId = value;
            _ = LoadAsync();
        }
    }

    /// <summary>
    /// Receives the ISBN value returned by <see cref="IsbnScannerPage"/> via
    /// Shell navigation query parameters.  Sets the ISBN on the ViewModel and
    /// triggers an automatic metadata lookup.
    /// </summary>
    public string? ScannedIsbn
    {
        set
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                _ = ApplyScannedIsbnAsync(value);
            }
        }
    }

    private async Task LoadAsync()
    {
        try
        {
            if (BindingContext is AddEditBookViewModel vm)
            {
                await vm.LoadCommand.ExecuteAsync(_bookId);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine(ex);
        }
    }

    private async Task ApplyScannedIsbnAsync(string isbn)
    {
        try
        {
            if (BindingContext is AddEditBookViewModel vm)
            {
                await vm.ApplyScannedIsbnAsync(isbn);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine(ex);
        }
    }
}
