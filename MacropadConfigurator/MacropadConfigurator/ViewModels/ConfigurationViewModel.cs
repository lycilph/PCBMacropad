using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using MacropadConfigurator.Models;
using MacropadConfigurator.Services;

namespace MacropadConfigurator.ViewModels;

public partial class ConfigurationViewModel : ObservableObject
{
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
}
