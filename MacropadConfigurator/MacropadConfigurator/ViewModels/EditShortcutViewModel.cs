using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using ICSharpCode.AvalonEdit.Document;
using MacropadConfigurator.Messages;
using MacropadConfigurator.Models;
using MacropadConfigurator.Services;
using Microsoft.CodeAnalysis.Scripting;

namespace MacropadConfigurator.ViewModels;

public partial class EditShortcutViewModel : ObservableObject
{
    private readonly CompilerService compilerService;
    private Script<object>? script;

    [ObservableProperty]
    private Shortcut? shortcut;
    
    [ObservableProperty]
    private string name = string.Empty;

    [ObservableProperty]
    private string text = string.Empty;

    [ObservableProperty]
    private string key = string.Empty;

    [ObservableProperty]
    private bool control = false;

    [ObservableProperty]
    private bool shift = false;

    [ObservableProperty]
    private bool alt = false;

    [ObservableProperty]
    private TextDocument document = new();

    [ObservableProperty]
    private string output = string.Empty;

    public EditShortcutViewModel(CompilerService compilerService)
    {
        this.compilerService = compilerService;
    }

    partial void OnShortcutChanged(Shortcut? value)
    {
        if (value != null)
        {
            Name = value.Name;
            Text = value.Text;
            Key = value.key.ToString();
            Control = value.Modifiers.HasFlag(ModifierKeys.Control);
            Shift = value.Modifiers.HasFlag(ModifierKeys.Shift);
            Alt = value.Modifiers.HasFlag(ModifierKeys.Alt);
            Document = new TextDocument(value.Script);
        }
    }

    [RelayCommand]
    private void Compile()
    {
        (var result, var msg, script) = compilerService.CompileScript(Document.Text);
        Output = result ? "Success" : msg;
    }

    [RelayCommand]
    private void Save()
    {
        compilerService.ExecuteScript(script);
    }

    [RelayCommand]
    private void Cancel()
    {
        WeakReferenceMessenger.Default.Send(new BackMessage());
    }
}
