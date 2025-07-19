using CommunityToolkit.Mvvm.ComponentModel;
using System.Windows.Input;

namespace MacropadConfigurator.Models;

public partial class MacropadButton : ObservableObject
{
    [ObservableProperty]
    private string text = string.Empty;

    [ObservableProperty]
    private Key key = Key.None;

    [ObservableProperty]
    private ModifierKeys modifier = ModifierKeys.None;
}
