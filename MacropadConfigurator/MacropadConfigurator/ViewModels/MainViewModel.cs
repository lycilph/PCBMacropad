using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NLog;

namespace MacropadConfigurator.ViewModels;

public partial class MainViewModel : ObservableObject, ILifecycleAware
{
    private static readonly Logger logger = LogManager.GetCurrentClassLogger();
    
    [ObservableProperty]
    private SettingsViewModel settingsViewModel;

    public MainViewModel(SettingsViewModel settings)
    {
        SettingsViewModel = settings;
    }

    public void OnClosing(CancelEventArgs e)
    {
        logger.Info("Hiding the main window");

        e.Cancel = true;
    }

    [RelayCommand]
    private void ToggleSettings()
    {
        logger.Info("Toggle settings");

        SettingsViewModel.IsOpen = !SettingsViewModel.IsOpen;
    }

    [RelayCommand]
    public void Exit()
    {
        logger.Info("Shutting down from the main window");

        App.Current.Shutdown();
    }
}
