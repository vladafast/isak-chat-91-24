using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using IsakChat.App.Models;
using IsakChat.App.Services;
using IsakChat.Shared.Dtos;

namespace IsakChat.App.ViewModels;

// Zajednicka logika za "opstu sobu" i za privatni cet - obe se ponasaju isto
// (ucitaj kes -> prikazi -> pokreni periodicno osvezavanje sa servera -> posalji poruku/sliku/reply).
public abstract partial class ChatViewModelBase : ViewModelBase
{
    protected readonly ApiClient Api;
    protected readonly LocalDatabase LocalDb;
    protected readonly SessionService Session;

    private CancellationTokenSource? _pollCts;
    private int _maxKnownId;

    public ObservableCollection<MessageItemViewModel> Messages { get; } = new();

    [ObservableProperty]
    private string draftText = string.Empty;

    [ObservableProperty]
    private FileResult? pendingImage;

    [ObservableProperty]
    private bool isReplying;

    [ObservableProperty]
    private string? replyingToSenderName;

    [ObservableProperty]
    private string? replyingToPreviewText;

    private int? _replyingToId;

    // Nas sopstveni (temirani) popup - zamena za native DisplayAlert/DisplayActionSheet koji
    // ne prati boje app-a nego izgled telefona. Vidi overlay u Controls/ChatBodyView.xaml.
    [ObservableProperty]
    private bool isPopupOpen;

    [ObservableProperty]
    private string popupTitle = string.Empty;

    [ObservableProperty]
    private string? popupMessage;

    public ObservableCollection<PopupOption> PopupOptions { get; } = new();

    private TaskCompletionSource<string?>? _popupTcs;

    protected abstract string ConversationKey { get; }
    protected abstract int? RecipientId { get; }

    protected ChatViewModelBase(ApiClient api, LocalDatabase localDb, SessionService session)
    {
        Api = api;
        LocalDb = localDb;
        Session = session;
    }

    public bool HasPendingImage => PendingImage is not null;

    partial void OnPendingImageChanged(FileResult? value) => OnPropertyChanged(nameof(HasPendingImage));

    public async Task LoadAsync()
    {
        IsBusy = true;
        ErrorMessage = null;
        try
        {
            var cached = await LocalDb.GetMessagesAsync(ConversationKey);
            Messages.Clear();
            foreach (var m in cached)
                Messages.Add(ToItemViewModel(m));

            _maxKnownId = cached.Count == 0 ? 0 : cached.Max(m => m.ServerId);

            await FetchAndAppendNewAsync();
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

    public void StartPolling()
    {
        StopPolling();
        _pollCts = new CancellationTokenSource();
        _ = PollLoopAsync(_pollCts.Token);
    }

    public void StopPolling()
    {
        _pollCts?.Cancel();
        _pollCts = null;
    }

    private async Task PollLoopAsync(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            try
            {
                await Task.Delay(TimeSpan.FromSeconds(3), token);
                await FetchAndAppendNewAsync();
            }
            catch (TaskCanceledException)
            {
                // normalno gasenje poll-a kad se stranica zatvori
            }
            catch (ApiException)
            {
                // server trenutno nedostupan - probacemo ponovo za 3s, ne rusimo app
            }
        }
    }

    private async Task FetchAndAppendNewAsync()
    {
        var fresh = await FetchNewAsync(_maxKnownId);
        if (fresh.Count == 0) return;

        var toCache = new List<LocalMessage>();
        foreach (var dto in fresh)
        {
            Messages.Add(ToItemViewModel(dto));
            toCache.Add(ToLocalMessage(dto));
            if (dto.Id > _maxKnownId) _maxKnownId = dto.Id;
        }
        await LocalDb.SaveMessagesAsync(toCache);
    }

    protected abstract Task<List<MessageDto>> FetchNewAsync(int afterId);

    // Prikaze nas custom popup (Controls/ChatBodyView.xaml overlay) i ceka da korisnik
    // izabere neku od opcija (ili zatvori popup dodirom van kartice -> vraca null).
    protected Task<string?> ShowPopupAsync(string title, string? message, string[] options, string? destructiveOption = null)
    {
        PopupTitle = title;
        PopupMessage = message;
        PopupOptions.Clear();
        foreach (var o in options)
            PopupOptions.Add(new PopupOption { Text = o, IsDestructive = o == destructiveOption });

        IsPopupOpen = true;
        _popupTcs = new TaskCompletionSource<string?>();
        return _popupTcs.Task;
    }

    [RelayCommand]
    private void PopupOptionSelected(PopupOption option)
    {
        IsPopupOpen = false;
        _popupTcs?.TrySetResult(option.Text);
    }

    [RelayCommand]
    private void PopupDismiss()
    {
        IsPopupOpen = false;
        _popupTcs?.TrySetResult(null);
    }

    // Tap na poruku - meni sa opcijama. "Obrisi poruku" se nudi samo za sopstvene poruke.
    [RelayCommand]
    private async Task MessageTappedAsync(MessageItemViewModel message)
    {
        if (message.IsDeleted) return;

        var options = message.IsMine
            ? new[] { "Odgovori", "Obriši poruku", "Otkaži" }
            : new[] { "Odgovori", "Otkaži" };

        var choice = await ShowPopupAsync("Poruka", null, options, destructiveOption: "Obriši poruku");

        switch (choice)
        {
            case "Odgovori":
                ReplyTo(message);
                break;
            case "Obriši poruku":
                await DeleteMessageAsync(message);
                break;
        }
    }

    private void ReplyTo(MessageItemViewModel message)
    {
        _replyingToId = message.Id;
        ReplyingToSenderName = message.SenderDisplayName;
        ReplyingToPreviewText = message.HasImage && !message.HasText ? "🖼 Slika" : message.Text;
        IsReplying = true;
    }

    private async Task DeleteMessageAsync(MessageItemViewModel message)
    {
        var choice = await ShowPopupAsync(
            "Brisanje poruke", "Da li sigurno želiš da obrišeš ovu poruku?",
            new[] { "Obriši", "Otkaži" }, destructiveOption: "Obriši");
        if (choice != "Obriši") return;

        try
        {
            await Api.DeleteMessageAsync(message.Id);

            var index = Messages.IndexOf(message);
            if (index >= 0)
                Messages[index] = MessageItemViewModel.AsDeleted(message);

            await LocalDb.MarkDeletedAsync(ConversationKey, message.Id);
        }
        catch (ApiException ex)
        {
            ErrorMessage = ex.Message;
        }
    }

    [RelayCommand]
    private void CancelReply()
    {
        _replyingToId = null;
        ReplyingToSenderName = null;
        ReplyingToPreviewText = null;
        IsReplying = false;
    }

    [RelayCommand]
    private async Task PickImageAsync()
    {
        try
        {
            var options = MediaPicker.Default.IsCaptureSupported
                ? new[] { "Iz galerije", "Kamera", "Otkaži" }
                : new[] { "Iz galerije", "Otkaži" };

            var choice = await ShowPopupAsync("Dodaj sliku", null, options);

            FileResult? result = choice switch
            {
                "Iz galerije" => await MediaPicker.Default.PickPhotoAsync(),
                "Kamera" => await MediaPicker.Default.CapturePhotoAsync(),
                _ => null
            };

            if (result is not null)
                PendingImage = result;
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Ne mogu da otvorim galeriju/kameru: {ex.Message}";
        }
    }

    [RelayCommand]
    private void ClearPendingImage() => PendingImage = null;

    [RelayCommand]
    private async Task SendAsync()
    {
        var text = DraftText.Trim();
        if (string.IsNullOrEmpty(text) && PendingImage is null) return;

        IsBusy = true;
        ErrorMessage = null;
        try
        {
            var sent = await Api.SendMessageAsync(text, PendingImage, _replyingToId, RecipientId);

            Messages.Add(ToItemViewModel(sent));
            await LocalDb.SaveMessagesAsync(new[] { ToLocalMessage(sent) });
            if (sent.Id > _maxKnownId) _maxKnownId = sent.Id;

            DraftText = string.Empty;
            PendingImage = null;
            CancelReplyCommand.Execute(null);
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

    private MessageItemViewModel ToItemViewModel(MessageDto dto) =>
        MessageItemViewModel.From(dto, Session.UserId, Session.ServerBaseUrl);

    private MessageItemViewModel ToItemViewModel(LocalMessage m) => new()
    {
        Id = m.ServerId,
        SenderDisplayName = m.SenderDisplayName,
        Text = m.Text,
        FullImageUrl = string.IsNullOrEmpty(m.ImageUrl) ? null : $"{Session.ServerBaseUrl.TrimEnd('/')}{m.ImageUrl}",
        IsMine = m.SenderId == Session.UserId,
        IsDeleted = m.IsDeleted,
        TimeText = m.SentAtUtc.ToLocalTime().ToString("HH:mm"),
        ReplySenderName = m.ReplyToSenderDisplayName,
        ReplyPreviewText = m.ReplyToPreviewText
    };

    private LocalMessage ToLocalMessage(MessageDto dto) => new()
    {
        ServerId = dto.Id,
        ConversationKey = ConversationKey,
        SenderId = dto.SenderId,
        SenderDisplayName = dto.SenderDisplayName,
        RecipientId = dto.RecipientId,
        Text = dto.IsDeleted ? MessageItemViewModel.DeletedPlaceholder : dto.Text,
        ImageUrl = dto.IsDeleted ? null : dto.ImageUrl,
        ReplyToId = dto.ReplyToId,
        ReplyToSenderDisplayName = dto.ReplyToSenderDisplayName,
        ReplyToPreviewText = dto.ReplyToPreviewText,
        SentAtUtc = dto.SentAtUtc,
        IsDeleted = dto.IsDeleted
    };
}
