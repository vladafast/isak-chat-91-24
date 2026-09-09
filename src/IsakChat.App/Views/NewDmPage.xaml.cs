using IsakChat.App.ViewModels;

namespace IsakChat.App.Views;

public partial class NewDmPage : ContentPage
{
    private readonly NewDmViewModel _vm;

    public NewDmPage(NewDmViewModel vm)
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
