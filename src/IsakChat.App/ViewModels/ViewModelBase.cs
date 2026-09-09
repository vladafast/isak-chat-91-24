using CommunityToolkit.Mvvm.ComponentModel;

namespace IsakChat.App.ViewModels;

// Zajednicka osnova za sve ViewModel-e - MVVM: View se bind-uje na ove property-je,
// ObservableObject (CommunityToolkit.Mvvm) automatski generise INotifyPropertyChanged.
public partial class ViewModelBase : ObservableObject
{
    [ObservableProperty]
    private bool isBusy;

    [ObservableProperty]
    private string? errorMessage;
}
