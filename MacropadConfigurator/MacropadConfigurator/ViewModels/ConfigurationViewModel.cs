using CommunityToolkit.Mvvm.ComponentModel;
using MacropadConfigurator.Models;
using MacropadConfigurator.Services;
using NLog;
using System.Collections.ObjectModel;

namespace MacropadConfigurator.ViewModels;

public partial class ConfigurationViewModel : ObservableObject, ILoadedAware
{
    private static readonly Logger logger = LogManager.GetCurrentClassLogger();

    private readonly MacropadManager macropadManager;

    [ObservableProperty]
    private ObservableCollection<MacropadLayer> layers;

    [ObservableProperty]
    private MacropadLayer selectedLayer;

    [ObservableProperty]
    private MacropadButton selectedButton;

    public ConfigurationViewModel(MacropadManager macropadManager)
    {
        this.macropadManager = macropadManager;

        layers = macropadManager.Layers;
        SelectedLayer = Layers.First();
        SelectedButton = SelectedLayer.Buttons.First();
    }

    public void OnLoaded()
    {
        logger.Info("ConfigurationViewModel loaded");
    }
}
