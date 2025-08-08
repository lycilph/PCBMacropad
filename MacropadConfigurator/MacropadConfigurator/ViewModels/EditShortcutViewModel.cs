using System.Windows.Input;
using System.Xml.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using ICSharpCode.AvalonEdit.Document;
using MacropadConfigurator.Messages;
using MacropadConfigurator.Models;
using MacropadConfigurator.Services;

namespace MacropadConfigurator.ViewModels;

public partial class EditShortcutViewModel : ObservableObject
{
    private Shortcut? shortcut;

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

    public void Set(Shortcut shortcut)
    {
        this.shortcut = shortcut;

        Text = shortcut.Text;
        Key = shortcut.Key.ToString();
        Control = shortcut.Modifiers.HasFlag(ModifierKeys.Control);
        Shift = shortcut.Modifiers.HasFlag(ModifierKeys.Shift);
        Alt = shortcut.Modifiers.HasFlag(ModifierKeys.Alt);
        Document = new TextDocument();
    }

    [RelayCommand]
    private async Task CompileAsync()
    {
        await Task.CompletedTask;

        //(var result, var msg, _) = await compilerService.CompileScriptAsync(Document.Text);
        //Output = result ? "Success" : msg;
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        if (shortcut == null) return;
        
        
        await Task.CompletedTask;
        shortcut.Text = Text;
        shortcut.Key = KeyParser.StringToKey(Key);
        shortcut.Modifiers = (Control ? ModifierKeys.Control : ModifierKeys.None) |
                             (Shift ? ModifierKeys.Shift : ModifierKeys.None) |
                             (Alt ? ModifierKeys.Alt : ModifierKeys.None);
        WeakReferenceMessenger.Default.Send(new NavigateToMainMessage());

        //(var result, var msg, var compiled_script) = await compilerService.CompileScriptAsync(Document.Text);
        //if (result && compiled_script != null)
        //{
        //    Shortcut.Name = Name;
        //    Shortcut.Text = Text;
        //    Shortcut.Key = KeyParser.StringToKey(Key);
        //    Shortcut.Modifiers = ConvertToModifierKeys();
        //    Shortcut.Script = Document.Text;
        //    Shortcut.CompiledScript = compiled_script;
        //    WeakReferenceMessenger.Default.Send(new BackMessage());
        //}
        //else
        //{
        //    Output = msg;
        //}

        shortcut = null;
        Output = string.Empty;
    }

    [RelayCommand]
    private void Cancel()
    {
        WeakReferenceMessenger.Default.Send(new NavigateToMainMessage());

        shortcut = null;
        Output = string.Empty;
    }
}
