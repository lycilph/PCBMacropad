using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using MacropadConfigurator.DTO;
using MacropadConfigurator.Models;

namespace MacropadConfigurator.Services;

public partial class MacropadManager : ObservableObject
{
    public ObservableCollection<MacropadLayer> Layers { get; private set; } = [];

    public MacropadManager()
    {
        Layers.Add(new MacropadLayer { Name = "Layer 1" });
        Layers.Add(new MacropadLayer { Name = "Layer 2" });
        Layers.Add(new MacropadLayer { Name = "Layer 3" });
    }

    public void Update(MacropadConfigurationDTO config)
    {
        if (config.layers == null || config.layers.Length != 3)
            throw new ArgumentException("Invalid configuration data");
        
        for (int i = 0; i < Layers.Count; i++)
        {
            var layer = Layers[i];
            var layerDto = config.layers[i];
            layer.Name = layerDto.name;
            layer.IsEnabled = layerDto.isEnabled;
            for (int j = 0; j < layer.Buttons.Count; j++)
            {
                var button = layer.Buttons[j];
                var buttonDto = layerDto.actions[j];
                button.Text = buttonDto.text;
                button.Key = KeyParser.HidToKey(buttonDto.key);
                button.Modifier = KeyParser.HidModifiersToKeys(buttonDto.modifier);
            }
        }
    }
}
