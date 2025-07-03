using System.Collections.ObjectModel;
using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using NLog;

namespace MacropadConfigurator.ViewModels;

public partial class MainViewModel : ObservableRecipient, IRecipient<string>, ILifecycleAware
{
    private static readonly Logger logger = LogManager.GetCurrentClassLogger();
    
    [ObservableProperty]
    private SettingsViewModel settingsViewModel;

    [ObservableProperty]
    private ShortcutsViewModel shortcutsViewModel;

    [ObservableProperty]
    private ConfigurationViewModel configurationViewModel;

    [ObservableProperty]
    private ObservableCollection<string> log = [];

    public MainViewModel(SettingsViewModel settings,
                         ShortcutsViewModel shortcutsViewModel,
                         ConfigurationViewModel configurationViewModel)
    {
        SettingsViewModel = settings;
        ShortcutsViewModel = shortcutsViewModel;
        ConfigurationViewModel = configurationViewModel;

        IsActive = true; // Needed to receive messages
    }

    public void OnClosing(CancelEventArgs e)
    {
        logger.Info("Hiding the main window");

        e.Cancel = true;
        App.Current.HideWindow();
    }

    public void Receive(string message)
    {
        Log.Add(message);
    }

    [RelayCommand]
    private void ToggleSettings()
    {
        logger.Info("Toggle settings");

        SettingsViewModel.IsOpen = !SettingsViewModel.IsOpen;
    }
}
