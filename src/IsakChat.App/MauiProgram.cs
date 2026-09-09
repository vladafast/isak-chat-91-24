using IsakChat.App.Services;
using IsakChat.App.ViewModels;
using IsakChat.App.Views;
using Microsoft.Extensions.Logging;

namespace IsakChat.App;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts => { });

#if DEBUG
        builder.Logging.AddDebug();
#endif

        // --- Servisi (svi singleton - jedna sesija/konekcija/baza po zivotu app-a) ---
        builder.Services.AddSingleton<LocalDatabase>();
        builder.Services.AddSingleton<SessionService>();
        builder.Services.AddSingleton<HttpClient>();
        builder.Services.AddSingleton<ApiClient>();

        // --- ViewModel-i (transient - novi po stranici) ---
        builder.Services.AddTransient<LoginViewModel>();
        builder.Services.AddTransient<RegisterViewModel>();
        builder.Services.AddTransient<PublicChatViewModel>();
        builder.Services.AddTransient<DmListViewModel>();
        builder.Services.AddTransient<DmChatViewModel>();
        builder.Services.AddTransient<NewDmViewModel>();
        builder.Services.AddTransient<ProfileViewModel>();

        // --- Stranice ---
        builder.Services.AddTransient<LoginPage>();
        builder.Services.AddTransient<RegisterPage>();
        builder.Services.AddTransient<PublicChatPage>();
        builder.Services.AddTransient<DmListPage>();
        builder.Services.AddTransient<DmChatPage>();
        builder.Services.AddTransient<NewDmPage>();
        builder.Services.AddTransient<ProfilePage>();

        return builder.Build();
    }
}
