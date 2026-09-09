using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using IsakChat.App.Services;

namespace IsakChat.App.ViewModels;

public partial class ProfileViewModel : ViewModelBase
{
    private readonly ApiClient _api;
    private readonly SessionService _session;

    [ObservableProperty]
    private string displayName = string.Empty;

    [ObservableProperty]
    private string username = string.Empty;

    [ObservableProperty]
    private string serverUrl = string.Empty;

    public ProfileViewModel(ApiClient api, SessionService session)
    {
        _api = api;
        _session = session;
    }

    public void Load()
    {
        DisplayName = _session.DisplayName;
        Username = _session.Username;
        ServerUrl = _session.ServerBaseUrl;
    }

    [RelayCommand]
    private async Task LogoutAsync()
    {
        IsBusy = true;
        try
        {
            await _api.LogoutAsync();
            await _session.SignOutAsync();
            await Shell.Current.GoToAsync("//login");
        }
        finally
        {
            IsBusy = false;
        }
    }
}
