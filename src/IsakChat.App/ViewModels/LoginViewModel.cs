using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using IsakChat.App.Services;

namespace IsakChat.App.ViewModels;

public partial class LoginViewModel : ViewModelBase
{
    private readonly ApiClient _api;
    private readonly SessionService _session;

    [ObservableProperty]
    private string username = string.Empty;

    [ObservableProperty]
    private string password = string.Empty;

    // Adresa servera - podrazumevano localhost (radi kad su server i klijent na istom racunaru,
    // ili kad se koristi "adb reverse tcp:5203 tcp:5203" za telefon prikacen preko USB-a).
    [ObservableProperty]
    private string serverUrl = "http://localhost:5203";

    public LoginViewModel(ApiClient api, SessionService session)
    {
        _api = api;
        _session = session;
    }

    [RelayCommand]
    private async Task LoginAsync()
    {
        if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
        {
            ErrorMessage = "Unesi korisnicko ime i lozinku.";
            return;
        }

        IsBusy = true;
        ErrorMessage = null;
        try
        {
            _session.SetServerBaseUrl(ServerUrl.Trim());
            var auth = await _api.LoginAsync(Username.Trim(), Password);
            await _session.SignInAsync(auth, ServerUrl.Trim());
            await Shell.Current.GoToAsync("//main/public");
        }
        catch (ApiException ex)
        {
            ErrorMessage = ex.Message;
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Ne mogu da se povezem na server ({ServerUrl}): {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task GoToRegisterAsync()
    {
        await Shell.Current.GoToAsync("//register");
    }
}
