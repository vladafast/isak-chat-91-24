using IsakChat.App.ViewModels;

namespace IsakChat.App.Views;

public partial class PublicChatPage : ContentPage
{
    private readonly PublicChatViewModel _vm;

    public PublicChatPage(PublicChatViewModel vm)
    {
        InitializeComponent();
        _vm = vm;
        BindingContext = vm;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _vm.LoadAsync();
        _vm.StartPolling();
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _vm.StopPolling();
    }
}
