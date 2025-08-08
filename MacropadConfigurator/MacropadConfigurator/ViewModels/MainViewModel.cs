using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using MacropadConfigurator.Messages;
using MacropadConfigurator.Models;
using MacropadConfigurator.Services;
using NLog;

namespace MacropadConfigurator.ViewModels;

public partial class MainViewModel : ObservableRecipient, IRecipient<LogMessage>
{
    private static readonly Logger logger = LogManager.GetCurrentClassLogger();

    private readonly ScriptService scriptService;

    public List<Layer> Layers { get; set; }

    [ObservableProperty]
    private Layer selectedLayer;

    [ObservableProperty]
    private ObservableCollection<string> log = [];

    public MainViewModel(ApplicationService applicationService, ScriptService scriptService)
    {
        this.scriptService = scriptService;

        Layers = applicationService.Layers;
        selectedLayer = Layers.First();

        IsActive = true;
    }

    public void Receive(LogMessage message)
    {
        logger.Info($"Got message: {message.Text}");
        Log.Add(message.Text);
    }

    [RelayCommand]
    private void EditLayerName()
    {
        WeakReferenceMessenger.Default.Send(new EditLayerMessage(SelectedLayer));
    }

    [RelayCommand]
    private void EditShortcut(Shortcut shortcut)
    {
        WeakReferenceMessenger.Default.Send(new EditShortcutMessage(shortcut));
    }

    [RelayCommand]
    private void ExecuteShortcut(Shortcut shortcut)
    {
        WeakReferenceMessenger.Default.Send(new LogMessage($"Executing script for shortcut [{shortcut.Text}]"));
        scriptService.ExecuteScriptAsync(shortcut.CompiledScript);
    }
}
