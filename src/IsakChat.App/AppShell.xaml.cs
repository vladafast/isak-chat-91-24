using IsakChat.App.Services;
using IsakChat.App.Views;

namespace IsakChat.App;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        // Rute koje se otvaraju navigacijom (GoToAsync), a nisu stalno vidljive u TabBar-u
        // (login/register su deo Shell strukture u AppShell.xaml - ne registruju se ovde)
        Routing.RegisterRoute("dmchat", typeof(DmChatPage));
        Routing.RegisterRoute("newdm", typeof(NewDmPage));

        Loaded += OnLoaded;
    }

    private async void OnLoaded(object? sender, EventArgs e)
    {
        Loaded -= OnLoaded;

        var session = Handler?.MauiContext?.Services.GetService<SessionService>();
        if (session is null) return;

        var restored = await session.TryRestoreAsync();
        await GoToAsync(restored ? "//main/public" : "//login");
    }
}
