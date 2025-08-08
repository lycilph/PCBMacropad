using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
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
                          SettingsService settingsService,
                          OverlayService overlayService,
                          CommunicationService communicationService,
                          IDialogCoordinator dialogCoordinator)
    {
        this.mainViewModel = mainViewModel;
        this.editShortcutViewModel = editShortcutViewModel;

        this.settingsService = settingsService;
        this.overlayService = overlayService;
        this.communicationService = communicationService;

        this.dialogCoordinator = dialogCoordinator;

        Content = mainViewModel;
        SettingsViewModel = settingsViewModel;

        overlayService.OverlayVisibilityChanged += OverlayVisibilityChanged;
        communicationService.ConfigurationLoaded += ConfigurationLoaded;
        communicationService.ConfigurationSaved += ConfigurationSaved;
        communicationService.ConfigurationReset += ConfigurationReset;

        IsActive = true;
    }

    private void OverlayVisibilityChanged(object? sender, EventArgs e)
    {
        OnPropertyChanged(nameof(IsOverlayVisible));
        OnPropertyChanged(nameof(IsSpinnerVisible));
    }

    private void ConfigurationLoaded(object? sender, EventArgs e)
    {
        App.Current.Dispatcher.BeginInvoke(() =>
        {
            overlayService.HideSpinner();
            WeakReferenceMessenger.Default.Send(new LogMessage("Configuration loaded"));
        });
    }

    private void ConfigurationSaved(object? sender, EventArgs e)
    {
        App.Current.Dispatcher.BeginInvoke(() =>
        {
            overlayService.HideSpinner();
            WeakReferenceMessenger.Default.Send(new LogMessage("Configuration saved"));
        });
    }

    private void ConfigurationReset(object? sender, EventArgs e)
    {
        App.Current.Dispatcher.BeginInvoke(() =>
        {
            overlayService.HideSpinner();
            WeakReferenceMessenger.Default.Send(new LogMessage("Configuration reset"));
        });
    }

    public void OnLoaded()
    {
        logger.Info("Shell is now loaded");
        WeakReferenceMessenger.Default.Send(new LogMessage("Application is ready"));
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
    private void UploadConfiguration()
    {
        WeakReferenceMessenger.Default.Send(new LogMessage("Uploading configuration to macropad"));
        overlayService.ShowSpinner();
        communicationService.SaveConfiguration();
    }

    [RelayCommand]
    private void DownloadConfiguration()
    {
        WeakReferenceMessenger.Default.Send(new LogMessage("Downloading configuration from macropad"));
        overlayService.ShowSpinner();
        communicationService.LoadConfiguration();
    }

    [RelayCommand]
    private void ResetConfiguration()
    {
        WeakReferenceMessenger.Default.Send(new LogMessage("Resetting configuration on macropad"));
        overlayService.ShowSpinner();
        communicationService.ResetConfiguration();
    }
}
