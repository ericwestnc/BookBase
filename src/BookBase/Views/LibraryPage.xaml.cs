using BookBase.Helpers;
using BookBase.ViewModels;

namespace BookBase.Views;

public partial class LibraryPage : ContentPage
{
    public LibraryPage()
    {
        InitializeComponent();
        BindingContext = ServiceHelper.GetService<LibraryViewModel>();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is LibraryViewModel vm)
        {
            await vm.LoadCommand.ExecuteAsync(null);
        }
    }
}
