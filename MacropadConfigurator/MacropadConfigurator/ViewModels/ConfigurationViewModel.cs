using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using MacropadConfigurator.DTO;
using MacropadConfigurator.Messages;
using MacropadConfigurator.Models;
using MacropadConfigurator.Services;
using NLog;
using System.Collections.ObjectModel;
using System.IO;
using System.Text.Json;

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

        Layers = macropadManager.Layers;
        SelectedLayer = Layers.First();
        SelectedButton = SelectedLayer.Buttons.First();

        communicationManager.ConfigurationLoaded += CommunicationManagerConfigurationLoaded;
    }

    private void CommunicationManagerConfigurationLoaded(object? sender, MacropadConfigurationDTO config)
    {
        App.Current.Dispatcher.BeginInvoke(() =>
        {
            logger.Info("Configuration loaded from macropad");
            WeakReferenceMessenger.Default.Send(new SetOverlayVisibilityMessage(false));
            WeakReferenceMessenger.Default.Send("Configuration loaded successfully.");

            // Debug - save the configuration to a file
            var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var appFolderPath = Path.Combine(appDataPath, "MacropadConfigurator");
            var filename = Path.Combine(appFolderPath, "macropad_configuration.json");
            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                IncludeFields = true
            };
            var json = JsonSerializer.Serialize(config, options);
            File.WriteAllText(filename, json);
        });
    }

    public async void OnLoaded()
    {
        //logger.Info("ConfigurationViewModel loaded");
        //WeakReferenceMessenger.Default.Send(new SetOverlayVisibilityMessage(true));

        //WeakReferenceMessenger.Default.Send("Looking for macropad...");
        //await Task.Run(communicationManager.FindMacropad);
        //WeakReferenceMessenger.Default.Send("Macropad found and connected.");

        //WeakReferenceMessenger.Default.Send("Loading configuration from macropad...");
        //communicationManager.LoadConfiguration();

        // Debug - load the configuration from a file
        await Task.Delay(1000); // Simulate delay for loading

        var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var appFolderPath = Path.Combine(appDataPath, "MacropadConfigurator");
        var filename = Path.Combine(appFolderPath, "macropad_configuration.json");
        var json = File.ReadAllText(filename);
        var options = new JsonSerializerOptions
        {
            WriteIndented = true,
            IncludeFields = true
        };
        var config = JsonSerializer.Deserialize<MacropadConfigurationDTO>(json, options);

        macropadManager.Update(config);
        Layers = macropadManager.Layers;
        SelectedLayer = Layers.First();
        SelectedButton = SelectedLayer.Buttons.First();
    }
}
