using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ControlzEx.Theming;
using MacropadConfigurator.Extensions;
using MacropadConfigurator.Services;
using NLog;

namespace MacropadConfigurator.ViewModels;

public partial class SettingsViewModel : ObservableObject
{
    private static readonly Logger logger = LogManager.GetCurrentClassLogger();
    
    private readonly ApplicationService application_service;
    private readonly SettingsService settings_service;

    [ObservableProperty]
    private bool isOpen = false;

    [ObservableProperty]
    private bool isLight = true;

    [ObservableProperty]
    private Theme selectedApplicationColor;

    public ObservableCollection<Theme> ApplicationColors { get; set; }

    public bool HideOnClose
    {
        get => settings_service.HideOnClose;
        set
        {
            if (settings_service.HideOnClose != value)
            {
                settings_service.HideOnClose = value;
                OnPropertyChanged(nameof(HideOnClose));
            }
        }
    }

    public bool RunOnStartup
    {
        get => settings_service.RunOnStartup;
        set
        {
            if (settings_service.RunOnStartup != value)
            {
                settings_service.RunOnStartup = value;
                OnPropertyChanged(nameof(RunOnStartup));
            }
        }
    }

    public SettingsViewModel(ApplicationService application_service, SettingsService settings_service)
    {
        this.application_service = application_service;
        this.settings_service = settings_service;

        ApplicationColors = settings_service.Themes.ToObservableCollection();
        
        var current = settings_service.GetCurrentColorScheme();
        SelectedApplicationColor = ApplicationColors.First(t => t.ColorScheme == current);

        IsLight = settings_service.IsCurrentBaseColorSchemeLight();
    }
    
    partial void OnIsLightChanged(bool value) => settings_service.SetBaseColorScheme(value);
    partial void OnSelectedApplicationColorChanged(Theme value) => settings_service.SetColorScheme(value.ColorScheme);

    public void ToggleOpen() => IsOpen = !IsOpen;

    [RelayCommand]
    public void ResetConfiguration()
    {
        logger.Info("Resetting configuration");
        application_service.ResetShortcuts();
    }

    [RelayCommand]
    public void Exit()
    {
        logger.Info("Shutting down from the main window");

        App.Current.Shutdown();
    }
}
