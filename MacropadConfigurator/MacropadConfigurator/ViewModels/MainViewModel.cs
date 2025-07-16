using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using MacropadConfigurator.Messages;
using NLog;

namespace MacropadConfigurator.ViewModels;

public partial class MainViewModel : ObservableRecipient, IRecipient<string>, IRecipient<SetOverlayVisibilityMessage>
{
    private static readonly Logger logger = LogManager.GetCurrentClassLogger();

    [ObservableProperty]
    private ShortcutsViewModel shortcutsViewModel;

    [ObservableProperty]
    private ConfigurationViewModel configurationViewModel;

    [ObservableProperty]
    private bool showOverlay = false;

    [ObservableProperty]
    private ObservableCollection<string> log = [];

    public MainViewModel(ShortcutsViewModel shortcutsViewModel,
                         ConfigurationViewModel configurationViewModel)
    {
        ShortcutsViewModel = shortcutsViewModel;
        ConfigurationViewModel = configurationViewModel;

        IsActive = true; // Needed to receive messages
    }

    public void Receive(string message)
    {
        logger.Debug($"Got application log message: {message}");
        Log.Add(message);
    }

    public void Receive(SetOverlayVisibilityMessage message)
    {
        ShowOverlay = message.Visible;
    }
}
