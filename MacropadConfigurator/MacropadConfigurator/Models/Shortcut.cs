using System.Diagnostics;
using System.Text.Json.Serialization;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.CodeAnalysis.Scripting;

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

    [ObservableProperty]
    public string script = string.Empty;

    [JsonIgnore]
    public Script<object>? CompiledScript { get; set; } = null;
}
