using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using IsakChat.App.Services;

namespace IsakChat.App.ViewModels;

public partial class RegisterViewModel : ViewModelBase
{
    private readonly ApiClient _api;
    private readonly SessionService _session;

    [ObservableProperty]
    private string username = string.Empty;

    [ObservableProperty]
    private string displayName = string.Empty;

    [ObservableProperty]
    private string password = string.Empty;

    [ObservableProperty]
    private string confirmPassword = string.Empty;

    [ObservableProperty]
    private string serverUrl = "http://localhost:5203";

    public RegisterViewModel(ApiClient api, SessionService session)
    {
        _api = api;
        _session = session;
    }

    [RelayCommand]
    private async Task RegisterAsync()
    {
        if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
        {
            ErrorMessage = "Korisnicko ime i lozinka su obavezni.";
            return;
        }
        if (Password != ConfirmPassword)
        {
            ErrorMessage = "Lozinke se ne poklapaju.";
            return;
        }

        IsBusy = true;
        ErrorMessage = null;
        try
        {
            _session.SetServerBaseUrl(ServerUrl.Trim());
            var auth = await _api.RegisterAsync(Username.Trim(), Password,
                string.IsNullOrWhiteSpace(DisplayName) ? Username.Trim() : DisplayName.Trim());
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
    private async Task GoBackAsync()
    {
        await Shell.Current.GoToAsync("//login");
    }
}
