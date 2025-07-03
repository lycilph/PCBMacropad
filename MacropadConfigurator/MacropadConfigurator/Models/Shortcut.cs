using System.Text.Json.Serialization;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;

namespace MacropadConfigurator.Models;

public partial class Shortcut : ObservableObject
{
    // The fields are public, so that they are serialized properly

    [ObservableProperty]
    public Key key;

    [ObservableProperty]
    public ModifierKeys modifiers;

    [ObservableProperty]
    public string name; // This is the name of the shortcut and will be displayed in desktop app

    [ObservableProperty]
    public string text; // This is the text that will be displayed on the macropad (limited to 5 chars)

    [ObservableProperty]
    public string script;

    //[JsonIgnore]
    //public Script<object> CompiledScript { get; set; } = null!;

    public Shortcut(Key key, string name, string text, string script) : this(key, ModifierKeys.None, name, text, script) { }

    [JsonConstructor]
    public Shortcut(Key key, ModifierKeys modifiers, string name, string text, string script)
    {
        Key = key;
        Modifiers = modifiers;
        Name = name;
        Text = text;
        Script = script;
    }
}
