using System.Collections.ObjectModel;
using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using MacropadConfigurator.DTO;
using MacropadConfigurator.Extensions;
using MacropadConfigurator.Messages;

namespace MacropadConfigurator.Models;

[DebuggerDisplay("Layer: {Name} [{IsEnabled}]")]
public partial class Layer : ObservableObject
{
    [ObservableProperty]
    private string name = string.Empty;

    [ObservableProperty]
    private bool isEnabled = true;

    public ObservableCollection<Shortcut> Shortcuts { get; set; } = Enumerable.Range(0,9).Select(_ => new Shortcut()).ToObservableCollection();

    partial void OnIsEnabledChanged(bool value)
    {
        var state = IsEnabled ? "Enabled" : "Disabled";
        WeakReferenceMessenger.Default.Send(new LogMessage($"Layer \"{Name}\" is now {state}"));
    }

    public void Update(MacropadLayerDTO dto)
    {
        Name = dto.name;
        IsEnabled = dto.isEnabled;

        for (int i = 0; i < Shortcuts.Count; i++)
            Shortcuts[i].Update(dto.buttons[i]);
    }

    public void Reset()
    {
        Name = "NA";
        IsEnabled = true;
        foreach (var shortcut in Shortcuts)
            shortcut.Reset();
    }
}
