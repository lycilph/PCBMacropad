using System.Diagnostics;
using System.Text.Json.Serialization;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using MacropadConfigurator.DTO;
using MacropadConfigurator.Services;
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

    public void Update(MacropadButtonDTO dto)
    {
        Text = dto.text;
        Key = KeyParser.HidToKey(dto.key);
        Modifiers = KeyParser.HidModifiersToKeys(dto.modifier);
    }

    public void Reset()
    {
        Text = "NA";
        Key = Key.None;
        Modifiers = ModifierKeys.None;
        Script = string.Empty;
        CompiledScript = null;
    }
}
