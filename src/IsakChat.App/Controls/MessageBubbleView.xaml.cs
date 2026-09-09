namespace IsakChat.App.Controls;

// Korisnicka (custom) kontrola - "balon" jedne poruke. Koristi se u CollectionView ItemTemplate-u
// i za opstu sobu i za privatne cetove (jedna kontrola, dva mesta upotrebe).
public partial class MessageBubbleView : ContentView
{
    public static readonly BindableProperty SenderNameProperty =
        BindableProperty.Create(nameof(SenderName), typeof(string), typeof(MessageBubbleView), string.Empty);

    public static readonly BindableProperty MessageTextProperty =
        BindableProperty.Create(nameof(MessageText), typeof(string), typeof(MessageBubbleView), string.Empty);

    public static readonly BindableProperty ImageUrlProperty =
        BindableProperty.Create(nameof(ImageUrl), typeof(string), typeof(MessageBubbleView), string.Empty);

    public static readonly BindableProperty IsMineProperty =
        BindableProperty.Create(nameof(IsMine), typeof(bool), typeof(MessageBubbleView), false);

    public static readonly BindableProperty TimeTextProperty =
        BindableProperty.Create(nameof(TimeText), typeof(string), typeof(MessageBubbleView), string.Empty);

    public static readonly BindableProperty ReplySenderNameProperty =
        BindableProperty.Create(nameof(ReplySenderName), typeof(string), typeof(MessageBubbleView), string.Empty);

    public static readonly BindableProperty ReplyPreviewTextProperty =
        BindableProperty.Create(nameof(ReplyPreviewText), typeof(string), typeof(MessageBubbleView), string.Empty);

    public static readonly BindableProperty IsDeletedProperty =
        BindableProperty.Create(nameof(IsDeleted), typeof(bool), typeof(MessageBubbleView), false);

    public string SenderName { get => (string)GetValue(SenderNameProperty); set => SetValue(SenderNameProperty, value); }
    public string MessageText { get => (string)GetValue(MessageTextProperty); set => SetValue(MessageTextProperty, value); }
    public string ImageUrl { get => (string)GetValue(ImageUrlProperty); set => SetValue(ImageUrlProperty, value); }
    public bool IsMine { get => (bool)GetValue(IsMineProperty); set => SetValue(IsMineProperty, value); }
    public string TimeText { get => (string)GetValue(TimeTextProperty); set => SetValue(TimeTextProperty, value); }
    public string ReplySenderName { get => (string)GetValue(ReplySenderNameProperty); set => SetValue(ReplySenderNameProperty, value); }
    public string ReplyPreviewText { get => (string)GetValue(ReplyPreviewTextProperty); set => SetValue(ReplyPreviewTextProperty, value); }
    public bool IsDeleted { get => (bool)GetValue(IsDeletedProperty); set => SetValue(IsDeletedProperty, value); }

    public MessageBubbleView()
    {
        InitializeComponent();
        // Unutrasnji elementi se bindiraju na bindable property-je OVE kontrole (ne na spoljasnji BindingContext)
        LayoutRoot.BindingContext = this;
    }
}
