using CommunityToolkit.Mvvm.ComponentModel;
using IsakChat.App.Services;
using IsakChat.Shared.Dtos;

namespace IsakChat.App.ViewModels;

// Privatni cet sa tacno jednim drugim korisnikom. OtherUserId/OtherUserName stizu
// kroz Shell navigaciju (query parametri), vidi AppShell rute i DmListPage.
[QueryProperty(nameof(OtherUserId), "userId")]
[QueryProperty(nameof(OtherUserName), "userName")]
public partial class DmChatViewModel : ChatViewModelBase
{
    [ObservableProperty]
    private int otherUserId;

    [ObservableProperty]
    private string otherUserName = string.Empty;

    protected override string ConversationKey => $"dm-{OtherUserId}";
    protected override int? RecipientId => OtherUserId;

    public DmChatViewModel(ApiClient api, LocalDatabase localDb, SessionService session)
        : base(api, localDb, session) { }

    protected override Task<List<MessageDto>> FetchNewAsync(int afterId) => Api.GetPrivateMessagesAsync(OtherUserId, afterId);
}
