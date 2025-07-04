using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using MacropadConfigurator.Messages;
using MacropadConfigurator.Models;

namespace MacropadConfigurator.ViewModels;

public partial class EditShortcutViewModel : ObservableObject
{
    [ObservableProperty]
    private Shortcut? shortcut;

    [RelayCommand]
    private void Back()
    {
        WeakReferenceMessenger.Default.Send(new BackMessage());
    }
}
