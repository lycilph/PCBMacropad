using System.Collections.ObjectModel;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using MacropadConfigurator.Messages;
using MacropadConfigurator.Models;
using MacropadConfigurator.Services;
using NLog;

namespace MacropadConfigurator.ViewModels;

public partial class ShortcutsViewModel : ObservableObject
{
    private static readonly Logger logger = LogManager.GetCurrentClassLogger();

    private readonly ShortcutManager shortcutManager;

    [ObservableProperty]
    private ObservableCollection<Shortcut> shortcuts;

    public ShortcutsViewModel(ShortcutManager shortcutManager)
    {
        this.shortcutManager = shortcutManager;
        Shortcuts = shortcutManager.Shortcuts;
    }

    [RelayCommand]
    private void Delete(Shortcut shortcut)
    {
        logger.Info($"Deleting {shortcut.Name}");
        WeakReferenceMessenger.Default.Send($"Deleting {shortcut.Name}");
    }

    [RelayCommand]
    private void Edit(Shortcut shortcut)
    {
        logger.Info($"Editing {shortcut.Name}");
        WeakReferenceMessenger.Default.Send($"Editing {shortcut.Name}");
        WeakReferenceMessenger.Default.Send(new EditShortcutMessage(shortcut));
    }

    [RelayCommand]
    private void Execute(Shortcut shortcut)
    {
        logger.Info($"Executing {shortcut.Name}");
        WeakReferenceMessenger.Default.Send($"Executing {shortcut.Name}");
    }
}
