using System.Collections.Specialized;
using IsakChat.App.ViewModels;

namespace IsakChat.App.Controls;

public partial class ChatBodyView : ContentView
{
    private ChatViewModelBase? _vm;

    public ChatBodyView()
    {
        InitializeComponent();
        BindingContextChanged += OnBindingContextChanged;
    }

    private void OnBindingContextChanged(object? sender, EventArgs e)
    {
        if (_vm is not null)
            _vm.Messages.CollectionChanged -= OnMessagesChanged;

        _vm = BindingContext as ChatViewModelBase;

        if (_vm is not null)
            _vm.Messages.CollectionChanged += OnMessagesChanged;
    }

    // Automatski skroluj na dno kad stigne nova poruka (poslata ili primljena)
    private void OnMessagesChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.Action != NotifyCollectionChangedAction.Add || _vm is null || _vm.Messages.Count == 0)
            return;

        MainThread.BeginInvokeOnMainThread(() =>
        {
            MessagesCollectionView.ScrollTo(_vm.Messages.Count - 1, animate: true);
        });
    }
}
