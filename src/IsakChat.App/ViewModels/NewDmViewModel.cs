using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.Input;
using IsakChat.App.Services;
using IsakChat.Shared.Dtos;

namespace IsakChat.App.ViewModels;

// Lista SVIH korisnika (osim mene) - biranje sa kim zapocinjem novi privatni cet.
public partial class NewDmViewModel : ViewModelBase
{
    private readonly ApiClient _api;

    public ObservableCollection<UserDto> Users { get; } = new();

    public NewDmViewModel(ApiClient api)
    {
        _api = api;
    }

    [RelayCommand]
    private async Task LoadAsync()
    {
        IsBusy = true;
        ErrorMessage = null;
        try
        {
            var users = await _api.GetUsersAsync();
            Users.Clear();
            foreach (var u in users)
                Users.Add(u);
        }
        catch (ApiException ex)
        {
            ErrorMessage = ex.Message;
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task SelectUserAsync(UserDto user)
    {
        // Vraca se preko trenutne (newdm) stranice pravo u cet, da Nazad iz ceta vodi na listu poruka
        await Shell.Current.GoToAsync($"../dmchat?userId={user.Id}&userName={Uri.EscapeDataString(user.DisplayName)}");
    }
}
