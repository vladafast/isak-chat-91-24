using IsakChat.App.ViewModels;

namespace IsakChat.App.Views;

public partial class DmChatPage : ContentPage
{
    private readonly DmChatViewModel _vm;

    public DmChatPage(DmChatViewModel vm)
    {
        InitializeComponent();
        _vm = vm;
        BindingContext = vm;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        // OtherUserId/OtherUserName su vec postavljeni preko Shell query parametara (QueryProperty) pre OnAppearing
        await _vm.LoadAsync();
        _vm.StartPolling();
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _vm.StopPolling();
    }
}
