using System.Collections.ObjectModel;
using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using MacropadConfigurator.Extensions;
using MacropadConfigurator.Messages;

namespace MacropadConfigurator.Models;

[DebuggerDisplay("Layer: {Name} [{IsEnabled}]")]
public partial class Layer : ObservableObject
{
    [ObservableProperty]
    private string name = string.Empty;

    [ObservableProperty]
    private bool isEnabled = false;

    public ObservableCollection<Shortcut> Shortcuts { get; set; } = Enumerable.Range(0,9).Select(_ => new Shortcut()).ToObservableCollection();

    partial void OnIsEnabledChanged(bool value)
    {
        var state = IsEnabled ? "Enabled" : "Disabled";
        WeakReferenceMessenger.Default.Send(new LogMessage($"Layer \"{Name}\" is now {state}"));
    }
}
