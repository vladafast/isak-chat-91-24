using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.Input;
using IsakChat.App.Services;

namespace IsakChat.App.ViewModels;

// Lista SVIH korisnika (osim mene) - biranje sa kim zapocinjem novi privatni cet.
public partial class NewDmViewModel : ViewModelBase
{
    private readonly ApiClient _api;

    public ObservableCollection<NewDmUserItem> Users { get; } = new();

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
            // Korisnici i postojeci cetovi su nezavisni pozivi ka serveru - ucitavamo ih
            // PARALELNO (Task.WhenAll) umesto redom, jedan ne zavisi od drugog pa nema
            // razloga da cekamo prvi da se zavrsi pre nego sto krene drugi.
            var usersTask = _api.GetUsersAsync();
            var conversationsTask = _api.GetConversationsAsync();
            await Task.WhenAll(usersTask, conversationsTask);

            var vecCetujemSa = conversationsTask.Result.Select(c => c.UserId).ToHashSet();

            Users.Clear();
            foreach (var u in usersTask.Result)
                Users.Add(NewDmUserItem.From(u, vecCetujemSa.Contains(u.Id)));
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
    private async Task SelectUserAsync(NewDmUserItem user)
    {
        // Vraca se preko trenutne (newdm) stranice pravo u cet, da Nazad iz ceta vodi na listu poruka
        await Shell.Current.GoToAsync($"../dmchat?userId={user.Id}&userName={Uri.EscapeDataString(user.DisplayName)}");
    }
}
