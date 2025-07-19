using System.Collections.ObjectModel;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using MacropadConfigurator.Extensions;

namespace MacropadConfigurator.Models;

public partial class MacropadLayer : ObservableObject
{
    [ObservableProperty]
    private string name = string.Empty;

    [ObservableProperty]
    private bool isEnabled = false;

    public ObservableCollection<MacropadButton> Buttons { get; private set; }

    public MacropadLayer()
    {
        Buttons = Enumerable
            .Range(0, 9)
            .Select(i => new MacropadButton { Text = $"{i}", Key = Key.None, Modifier = ModifierKeys.None })
            .ToObservableCollection();
    }
}
