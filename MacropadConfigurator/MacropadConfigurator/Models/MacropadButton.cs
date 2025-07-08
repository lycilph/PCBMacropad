using CommunityToolkit.Mvvm.ComponentModel;

namespace MacropadConfigurator.Models;

public partial class MacropadButton : ObservableObject
{
    [ObservableProperty]
    private string text = string.Empty;

    [ObservableProperty]
    private string key = string.Empty;

    [ObservableProperty]
    private string modifier = string.Empty;
}
