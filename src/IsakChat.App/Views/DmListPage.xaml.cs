using IsakChat.App.ViewModels;

namespace IsakChat.App.Views;

public partial class DmListPage : ContentPage
{
    private readonly DmListViewModel _vm;

    public DmListPage(DmListViewModel vm)
    {
        InitializeComponent();
        _vm = vm;
        BindingContext = vm;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _vm.LoadCommand.ExecuteAsync(null);
    }
}
