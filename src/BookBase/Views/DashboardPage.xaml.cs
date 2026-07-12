using BookBase.Helpers;
using BookBase.ViewModels;

namespace BookBase.Views;

public partial class DashboardPage : ContentPage
{
    public DashboardPage()
    {
        InitializeComponent();
        BindingContext = ServiceHelper.GetService<DashboardViewModel>();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is DashboardViewModel vm)
        {
            await vm.LoadCommand.ExecuteAsync(null);
        }
    }
}
