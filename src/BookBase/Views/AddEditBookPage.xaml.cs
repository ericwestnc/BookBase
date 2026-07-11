using BookBase.Helpers;
using BookBase.ViewModels;

namespace BookBase.Views;

public partial class AddEditBookPage : ContentPage
{
    public AddEditBookPage()
    {
        InitializeComponent();
        BindingContext = ServiceHelper.GetService<AddEditBookViewModel>();
    }
}
