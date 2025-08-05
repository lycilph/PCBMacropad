using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using MacropadConfigurator.Extensions;
using MacropadConfigurator.Services;

namespace MacropadConfigurator.Models;

[DebuggerDisplay("Layer: {Name} [{IsEnabled}]")]
public partial class Layer : ObservableObject
{
    [ObservableProperty]
    private string name = string.Empty;

    [ObservableProperty]
    private bool isEnabled = false;

    public ObservableCollection<Shortcut> Shortcuts { get; private set; }

    public Layer()
    {
        Shortcuts = Enumerable
            .Range(1, 9)
            .Select(i => new Shortcut { Text = $"Button{i}", Key = KeyParser.StringToKey("a"), Modifiers = ModifierKeys.None })
            .ToObservableCollection();
    }
}
