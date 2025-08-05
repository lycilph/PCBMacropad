using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MacropadConfigurator.Services;
using NLog;

namespace MacropadConfigurator.ViewModels;

public partial class ShellViewModel : ObservableObject
{
    private static readonly Logger logger = LogManager.GetCurrentClassLogger();

    private readonly OverlayService overlayService;

    public bool IsOverlayVisible => overlayService.IsOverlayVisible;
    
    [ObservableProperty]
    private SettingsViewModel settingsViewModel;

    [ObservableProperty]
    private ObservableObject content;

    public ShellViewModel(MainViewModel mainViewModel, SettingsViewModel settingsViewModel, OverlayService overlayService)
    {
        this.overlayService = overlayService;

        Content = mainViewModel;
        SettingsViewModel = settingsViewModel;

        overlayService.OverlayVisibilityChanged += (s, e) => OnPropertyChanged(nameof(IsOverlayVisible));
    }

    [RelayCommand]
    private void ToggleSettings()
    {
        logger.Info("Toggle settings");

        overlayService.ToggleOverlay();
        SettingsViewModel.ToggleOpen();
    }
}
