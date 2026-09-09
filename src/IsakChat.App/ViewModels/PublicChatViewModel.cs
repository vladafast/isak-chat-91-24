using IsakChat.App.Services;
using IsakChat.Shared.Dtos;

namespace IsakChat.App.ViewModels;

// Opsta ("zajednicka") soba - svi ulogovani korisnici vide iste poruke.
public class PublicChatViewModel : ChatViewModelBase
{
    protected override string ConversationKey => "public";
    protected override int? RecipientId => null;

    public PublicChatViewModel(ApiClient api, LocalDatabase localDb, SessionService session)
        : base(api, localDb, session) { }

    protected override Task<List<MessageDto>> FetchNewAsync(int afterId) => Api.GetPublicMessagesAsync(afterId);
}
