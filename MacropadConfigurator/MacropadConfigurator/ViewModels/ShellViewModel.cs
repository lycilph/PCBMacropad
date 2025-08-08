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
    private readonly IDialogCoordinator dialogCoordinator;

    public bool IsOverlayVisible => overlayService.IsOverlayVisible;
    
    [ObservableProperty]
    private SettingsViewModel settingsViewModel;

    [ObservableProperty]
    private ObservableObject content;

    public ShellViewModel(MainViewModel mainViewModel,
                          EditShortcutViewModel editShortcutViewModel,
                          SettingsViewModel settingsViewModel,
                          SettingsService settingsService,
                          OverlayService overlayService,
                          IDialogCoordinator dialogCoordinator)
    {
        this.mainViewModel = mainViewModel;
        this.editShortcutViewModel = editShortcutViewModel;

        this.settingsService = settingsService;
        this.overlayService = overlayService;
        this.dialogCoordinator = dialogCoordinator;

        Content = mainViewModel;
        SettingsViewModel = settingsViewModel;

        overlayService.OverlayVisibilityChanged += (s, e) => OnPropertyChanged(nameof(IsOverlayVisible));

        IsActive = true;
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
}
