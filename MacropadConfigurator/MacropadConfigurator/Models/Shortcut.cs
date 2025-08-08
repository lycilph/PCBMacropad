using System.Diagnostics;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;

namespace MacropadConfigurator.Models;

[DebuggerDisplay("Shortcut: {Text} [{Modifiers} {Key}]")]
public partial class Shortcut : ObservableObject
{
    [ObservableProperty]
    private string text = "NA";

    [ObservableProperty]
    public Key key;

    [ObservableProperty]
    public ModifierKeys modifiers;
}
