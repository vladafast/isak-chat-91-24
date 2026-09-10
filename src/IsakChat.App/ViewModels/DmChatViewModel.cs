using System.Text;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
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

    // Izvoz privatnog razgovora u .txt fajl na uredjaju - rad sa fajlovima lokalno na uredjaju
    // (odvojeno od SQLite baze). Namerno samo za privatne cetove, ne i za opstu sobu.
    [RelayCommand]
    private async Task ExportAsync()
    {
        if (Messages.Count == 0)
        {
            await ShowPopupAsync("Izvoz razgovora", "Nema poruka za izvoz.", new[] { "OK" });
            return;
        }

        IsBusy = true;
        ErrorMessage = null;
        try
        {
            var sb = new StringBuilder();
            sb.AppendLine($"Privatni cet sa korisnikom: {OtherUserName}");
            sb.AppendLine($"Izvezeno: {DateTime.Now:dd.MM.yyyy. HH:mm}");
            sb.AppendLine(new string('-', 40));

            foreach (var m in Messages)
            {
                var posiljalac = m.IsMine ? "Ja" : m.SenderDisplayName;
                var sadrzaj = m.IsDeleted
                    ? "[poruka obrisana]"
                    : m.HasImage
                        ? $"[slika]{(m.HasText ? " " + m.Text : string.Empty)}"
                        : m.Text;

                sb.AppendLine($"[{m.TimeText}] {posiljalac}: {sadrzaj}");
            }

            var folder = Path.Combine(FileSystem.AppDataDirectory, "Izvoz");
            Directory.CreateDirectory(folder);

            var fileName = $"cet-{SanitizeFileName(OtherUserName)}-{DateTime.Now:yyyyMMdd-HHmmss}.txt";
            var fullPath = Path.Combine(folder, fileName);

            await File.WriteAllTextAsync(fullPath, sb.ToString());

            await ShowPopupAsync("Izvoz razgovora", $"Sačuvano u:\n{fullPath}", new[] { "OK" });
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Izvoz nije uspeo: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private static string SanitizeFileName(string name)
    {
        foreach (var c in Path.GetInvalidFileNameChars())
            name = name.Replace(c, '_');
        return name;
    }
}
