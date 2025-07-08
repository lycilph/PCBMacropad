using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
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
        Layers.Add(new MacropadLayer { Name = "Layer 4" });
    }
}
