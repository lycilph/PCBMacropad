using System.ComponentModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using MacropadConfigurator.DTO;
using MacropadConfigurator.Messages;
using MacropadConfigurator.Services;
using MahApps.Metro.Controls.Dialogs;
using NLog;

namespace MacropadConfigurator.ViewModels;

public partial class ShellViewModel 
    : ObservableRecipient, 
      IWindowLifecycleAware, 
      IRecipient<EditLayerMessage>, 
      IRecipient<EditShortcutMessage>, 
      IRecipient<NavigateToMainMessage>
{
    private static readonly Logger logger = LogManager.GetCurrentClassLogger();

    private readonly MainViewModel mainViewModel;
    private readonly EditShortcutViewModel editShortcutViewModel;

    private readonly ApplicationService applicationService;
    private readonly SettingsService settingsService;
    private readonly OverlayService overlayService;
    private readonly CommunicationService communicationService;
    
    private readonly IDialogCoordinator dialogCoordinator;

    public bool IsOverlayVisible => overlayService.IsOverlayVisible;
    public bool IsSpinnerVisible => overlayService.IsSpinnerVisible;
    
    [ObservableProperty]
    private SettingsViewModel settingsViewModel;

    [ObservableProperty]
    private ObservableObject content;

    public ShellViewModel(MainViewModel mainViewModel,
                          EditShortcutViewModel editShortcutViewModel,
                          SettingsViewModel settingsViewModel,
                          ApplicationService applicationService,
                          SettingsService settingsService,
                          OverlayService overlayService,
                          CommunicationService communicationService,
                          IDialogCoordinator dialogCoordinator)
    {
        this.mainViewModel = mainViewModel;
        this.editShortcutViewModel = editShortcutViewModel;

        this.applicationService = applicationService;
        this.settingsService = settingsService;
        this.overlayService = overlayService;
        this.communicationService = communicationService;

        this.dialogCoordinator = dialogCoordinator;

        Content = mainViewModel;
        SettingsViewModel = settingsViewModel;

        overlayService.OverlayVisibilityChanged += OverlayVisibilityChanged;
        communicationService.ConfigurationLoaded += ConfigurationLoaded;

        IsActive = true;
    }

    private void OverlayVisibilityChanged(object? sender, EventArgs e)
    {
        OnPropertyChanged(nameof(IsOverlayVisible));
        OnPropertyChanged(nameof(IsSpinnerVisible));
    }

    private void ConfigurationLoaded(object? sender, MacropadConfigurationDTO config)
    {
        App.Current.Dispatcher.BeginInvoke(() =>
        {
            overlayService.HideSpinner();
            WeakReferenceMessenger.Default.Send(new LogMessage("Configuration loaded"));
            applicationService.UpdateShortcuts(config);
        });
    }

    public void OnLoaded()
    {
        logger.Info("Shell is now loaded");
        WeakReferenceMessenger.Default.Send(new LogMessage("Application is ready"));

        overlayService.ShowSpinner();
        applicationService.InitializeScripts();
        overlayService.HideSpinner();
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

    public async void Receive(EditLayerMessage message)
    {
        var settings = new MetroDialogSettings()
        {
            DefaultText = message.Layer.Name
        };

        var name = await dialogCoordinator.ShowInputAsync(this, "Layer Name", "Input new layer name (max 12 chars)", settings);
        if (!string.IsNullOrWhiteSpace(name))
        {
            message.Layer.Name = name.Substring(0, name.Length < 12 ? name.Length : 12);
            WeakReferenceMessenger.Default.Send(new LogMessage($"Layer name changed to \"{message.Layer.Name}\""));
        }
    }

    public void Receive(EditShortcutMessage message)
    {
        editShortcutViewModel.Set(message.Shortcut);
        Content = editShortcutViewModel;
    }

    public void Receive(NavigateToMainMessage message)
    {
        Content = mainViewModel;
    }

    [RelayCommand]
    private void ToggleSettings()
    {
        logger.Info("Toggle settings");
        WeakReferenceMessenger.Default.Send(new LogMessage("Toggling settings"));

        overlayService.ToggleOverlay();
        SettingsViewModel.ToggleOpen();
    }

    [RelayCommand]
    private async Task UploadConfigurationAsync()
    {
        WeakReferenceMessenger.Default.Send(new LogMessage("Uploading configuration to macropad"));
        overlayService.ShowSpinner();

        WeakReferenceMessenger.Default.Send(new LogMessage("Searching for macropad"));
        var result = await communicationService.FindMacropad();

        if (result)
        {
            var config = applicationService.GetConfiguration();
            await Task.Run(() => communicationService.SaveConfiguration(config));
            WeakReferenceMessenger.Default.Send(new LogMessage("Configuration uploaded successfully"));
        }
        else
        {
            WeakReferenceMessenger.Default.Send(new LogMessage("Macropad not found. Please ensure it is connected and try again."));
        }

        overlayService.HideSpinner();
    }

    [RelayCommand]
    private async Task DownloadConfigurationAsync()
    {
        overlayService.ShowSpinner();
        
        WeakReferenceMessenger.Default.Send(new LogMessage("Searching for macropad"));
        var result = await communicationService.FindMacropad();
        
        if (result)
        {
            WeakReferenceMessenger.Default.Send(new LogMessage("Macropad found"));
            WeakReferenceMessenger.Default.Send(new LogMessage("Downloading configuration from macropad"));
            communicationService.LoadConfiguration();
        }
        else
        {
            WeakReferenceMessenger.Default.Send(new LogMessage("Macropad not found. Please ensure it is connected and try again."));
            overlayService.HideSpinner();
        }
    }

    [RelayCommand]
    private async Task ResetConfigurationAsync()
    {
        WeakReferenceMessenger.Default.Send(new LogMessage("Resetting configuration on macropad"));
        overlayService.ShowSpinner();

        WeakReferenceMessenger.Default.Send(new LogMessage("Searching for macropad"));
        var result = await communicationService.FindMacropad();

        if (result)
        {
            var config = applicationService.GetConfiguration();
            await Task.Run(communicationService.ResetConfiguration);
            WeakReferenceMessenger.Default.Send(new LogMessage("Configuration reset successfully"));
        }
        else
        {
            WeakReferenceMessenger.Default.Send(new LogMessage("Macropad not found. Please ensure it is connected and try again."));
        }

        overlayService.HideSpinner();
    }
}
