using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using MacropadConfigurator.Messages;
using MacropadConfigurator.Models;
using MacropadConfigurator.Services;
using MahApps.Metro.Controls.Dialogs;
using NLog;

namespace MacropadConfigurator.ViewModels;

public partial class ShortcutsViewModel : ObservableObject
{
    private static readonly Logger logger = LogManager.GetCurrentClassLogger();

    private readonly ShortcutManager shortcutManager;
    private readonly CompilerService compilerService;

    [ObservableProperty]
    private ObservableCollection<Shortcut> shortcuts;

    public ShortcutsViewModel(ShortcutManager shortcutManager, CompilerService compilerService)
    {
        this.shortcutManager = shortcutManager;
        this.compilerService = compilerService;

        Shortcuts = shortcutManager.Shortcuts;
    }

    [RelayCommand]
    private void Create()
    {
        var shortcut = shortcutManager.Create();
        WeakReferenceMessenger.Default.Send(new EditShortcutMessage(shortcut));
    }

    [RelayCommand]
    private async Task DeleteAsync(Shortcut shortcut)
    {
        var result = await WeakReferenceMessenger.Default.Send(new DeleteConfirmationRequestMessage(shortcut.Name));

        if (result == MessageDialogResult.Affirmative)
        {
            logger.Info($"Deleting {shortcut.Name}");
            WeakReferenceMessenger.Default.Send($"Deleting {shortcut.Name}");
            shortcutManager.Delete(shortcut);
        }
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
        compilerService.ExecuteScript(shortcut.CompiledScript);
    }
}
