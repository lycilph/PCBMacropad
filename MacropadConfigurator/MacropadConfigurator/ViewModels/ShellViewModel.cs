using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using MacropadConfigurator.Messages;
using MacropadConfigurator.Services;
using NLog;

namespace MacropadConfigurator.ViewModels;

public partial class ShellViewModel : ObservableObject, IWindowLifecycleAware
{
    private static readonly Logger logger = LogManager.GetCurrentClassLogger();

    private readonly SettingsService settingsService;
    private readonly OverlayService overlayService;

    public bool IsOverlayVisible => overlayService.IsOverlayVisible;
    
    [ObservableProperty]
    private SettingsViewModel settingsViewModel;

    [ObservableProperty]
    private ObservableObject content;

    public ShellViewModel(MainViewModel mainViewModel,
                          SettingsViewModel settingsViewModel,
                          SettingsService settingsService,
                          OverlayService overlayService)
    {
        this.settingsService = settingsService;
        this.overlayService = overlayService;

        Content = mainViewModel;
        SettingsViewModel = settingsViewModel;

        overlayService.OverlayVisibilityChanged += (s, e) => OnPropertyChanged(nameof(IsOverlayVisible));
    }

    public void OnClosing(CancelEventArgs e)
    {
        if (settingsService.HideOnClose)
        {
            logger.Info("Hiding the main window");
            e.Cancel = true;
            App.Current.HideWindow();
        }
    }

    [RelayCommand]
    private void ToggleSettings()
    {
        logger.Info("Toggle settings");
        WeakReferenceMessenger.Default.Send(new LogMessage("Toggling settings"));

        overlayService.ToggleOverlay();
        SettingsViewModel.ToggleOpen();
    }
}
