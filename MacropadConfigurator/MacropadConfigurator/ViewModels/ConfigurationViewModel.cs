using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using MacropadConfigurator.Messages;
using MacropadConfigurator.Models;
using MacropadConfigurator.Services;
using NLog;
using System.Collections.ObjectModel;

namespace MacropadConfigurator.ViewModels;

public partial class ConfigurationViewModel : ObservableObject, ILoadedAware
{
    private static readonly Logger logger = LogManager.GetCurrentClassLogger();

    private readonly MacropadManager macropadManager;
    private readonly CommunicationManager communicationManager;

    [ObservableProperty]
    private ObservableCollection<MacropadLayer> layers;

    [ObservableProperty]
    private MacropadLayer selectedLayer;

    [ObservableProperty]
    private MacropadButton selectedButton;

    public ConfigurationViewModel(MacropadManager macropadManager, CommunicationManager communicationManager)
    {
        this.macropadManager = macropadManager;
        this.communicationManager = communicationManager;

        layers = macropadManager.Layers;
        SelectedLayer = Layers.First();
        SelectedButton = SelectedLayer.Buttons.First();
    }

    public async void OnLoaded()
    {
        logger.Info("ConfigurationViewModel loaded");
        WeakReferenceMessenger.Default.Send(new SetOverlayVisibilityMessage(true));
        
        WeakReferenceMessenger.Default.Send("Looking for macropad...");
        //await Task.Delay(1000); // Simulate some delay for loading
        await Task.Run(communicationManager.FindMacropad);
        WeakReferenceMessenger.Default.Send("Macropad found and connected.");

        WeakReferenceMessenger.Default.Send("Loading configuration from macropad...");
        communicationManager.LoadConfiguration();
        //await Task.Delay(1000); // Simulate some delay for loading
        WeakReferenceMessenger.Default.Send("Configuration loaded");

        WeakReferenceMessenger.Default.Send(new SetOverlayVisibilityMessage(false));
    }
}
