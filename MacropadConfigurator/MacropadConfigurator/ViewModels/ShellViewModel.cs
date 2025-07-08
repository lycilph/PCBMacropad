using System.ComponentModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using MacropadConfigurator.Messages;
using MahApps.Metro.Controls.Dialogs;
using NLog;

namespace MacropadConfigurator.ViewModels;

public partial class ShellViewModel : 
    ObservableRecipient, 
    IRecipient<EditShortcutMessage>, 
    IRecipient<BackMessage>, 
    ILifecycleAware
{
    private static readonly Logger logger = LogManager.GetCurrentClassLogger();

    private readonly MainViewModel mainViewModel;
    private readonly EditShortcutViewModel editShortcutViewModel;
    private readonly IDialogCoordinator dialogCoordinator;

    [ObservableProperty]
    private SettingsViewModel settingsViewModel;

    [ObservableProperty]
    private ObservableObject content;

    public ShellViewModel(MainViewModel mainViewModel,
                          EditShortcutViewModel editShortcutViewModel,
                          SettingsViewModel settingsViewModel,
                          IDialogCoordinator dialogCoordinator)
    {
        this.mainViewModel = mainViewModel;
        this.editShortcutViewModel = editShortcutViewModel;
        this.dialogCoordinator = dialogCoordinator;
        SettingsViewModel = settingsViewModel;

        Content = mainViewModel;

        //IsActive = true;
        WeakReferenceMessenger.Default.RegisterAll(this);
        WeakReferenceMessenger.Default.Register<DeleteConfirmationRequestMessage>(this, (r, m) => m.Reply(Receive(m)));
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

    public async Task<MessageDialogResult> Receive(DeleteConfirmationRequestMessage message)
    {
        return await ShowMessageAsync("Warning", $"Are you sure you want to delete the shortcut [{message.Name}]?", MessageDialogStyle.AffirmativeAndNegative);
    }

    public async Task<MessageDialogResult> ShowMessageAsync(string title, string msg, MessageDialogStyle style)
    {
        return await dialogCoordinator.ShowMessageAsync(this, title, msg, style);
    }

    [RelayCommand]
    private void ToggleSettings()
    {
        logger.Info("Toggle settings");

        SettingsViewModel.IsOpen = !SettingsViewModel.IsOpen;
    }
}
