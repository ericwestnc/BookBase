using BookBase.Helpers;
using BookBase.ViewModels;

namespace BookBase.Views;

[QueryProperty(nameof(BookId), "bookId")]
public partial class BookDetailsPage : ContentPage
{
    private int _bookId;

    public BookDetailsPage()
    {
        InitializeComponent();
        BindingContext = ServiceHelper.GetService<BookDetailsViewModel>();
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

    private async Task LoadAsync()
    {
        try
        {
            if (BindingContext is BookDetailsViewModel vm)
            {
                await vm.LoadCommand.ExecuteAsync(_bookId);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine(ex);
        }
    }
}
