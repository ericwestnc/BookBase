using BookBase.Helpers;
using BookBase.ViewModels;

namespace BookBase.Views;

[QueryProperty(nameof(BookId), "bookId")]
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
}
