using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using MacropadConfigurator.Messages;
using NLog;

namespace MacropadConfigurator.ViewModels;

public partial class ShellViewModel : ObservableRecipient, IRecipient<EditShortcutMessage>, IRecipient<BackMessage>, ILifecycleAware
{
    private static readonly Logger logger = LogManager.GetCurrentClassLogger();

    private readonly MainViewModel mainViewModel;
    private readonly EditShortcutViewModel editShortcutViewModel;

    [ObservableProperty]
    private SettingsViewModel settingsViewModel;

    [ObservableProperty]
    private ObservableObject content;

    public ShellViewModel(MainViewModel mainViewModel,
                          EditShortcutViewModel editShortcutViewModel,
                          SettingsViewModel settingsViewModel)
    {
        this.mainViewModel = mainViewModel;
        this.editShortcutViewModel = editShortcutViewModel;
        SettingsViewModel = settingsViewModel;

        Content = mainViewModel;
        IsActive = true;
    }

    public void OnClosing(CancelEventArgs e)
    {
        logger.Info("Hiding the main window");

        e.Cancel = true;
        App.Current.HideWindow();
    }

    public void Receive(EditShortcutMessage message)
    {
        editShortcutViewModel.Shortcut = message.Shortcut;
        Content = editShortcutViewModel;
    }

    public void Receive(BackMessage message)
    {
        Content = mainViewModel;
    }

    [RelayCommand]
    private void ToggleSettings()
    {
        logger.Info("Toggle settings");

        SettingsViewModel.IsOpen = !SettingsViewModel.IsOpen;
    }
}
