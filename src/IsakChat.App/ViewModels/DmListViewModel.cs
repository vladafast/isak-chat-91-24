using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using IsakChat.App.Services;
using IsakChat.Shared.Dtos;

namespace IsakChat.App.ViewModels;

// Lista postojecih privatnih cetova ("Poruke" tab).
public partial class DmListViewModel : ViewModelBase
{
    private readonly ApiClient _api;

    public ObservableCollection<ConversationDto> Conversations { get; } = new();

    public DmListViewModel(ApiClient api)
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
            var conversations = await _api.GetConversationsAsync();
            Conversations.Clear();
            foreach (var c in conversations)
                Conversations.Add(c);
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
    private async Task OpenChatAsync(ConversationDto conversation)
    {
        await Shell.Current.GoToAsync($"dmchat?userId={conversation.UserId}&userName={Uri.EscapeDataString(conversation.DisplayName)}");
    }

    [RelayCommand]
    private async Task NewChatAsync()
    {
        await Shell.Current.GoToAsync("newdm");
    }
}
