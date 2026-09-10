namespace IsakChat.App.ViewModels;

// Jedno dugme u nasem custom popup-u (zamena za native ActionSheet/Alert dugme).
public class PopupOption
{
    public string Text { get; init; } = string.Empty;
    public bool IsDestructive { get; init; }
}
