using System.Threading.Tasks;
using System.Windows.Input;
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
    private readonly CompilerService compilerService;

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
    
    private ModifierKeys ConvertToModifierKeys()
    {
        return (Control ? ModifierKeys.Control : ModifierKeys.None) |
               (Shift ? ModifierKeys.Shift : ModifierKeys.None) |
               (Alt ? ModifierKeys.Alt : ModifierKeys.None);
    }

    [RelayCommand]
    private async Task CompileAsync()
    {
        (var result, var msg, _) = await compilerService.CompileScriptAsync(Document.Text);
        Output = result ? "Success" : msg;
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        if (Shortcut == null) return;

        (var result, var msg, var compiled_script) = await compilerService.CompileScriptAsync(Document.Text);
        if (result && compiled_script != null)
        {
            Shortcut.Name = Name;
            Shortcut.Text = Text;
            Shortcut.Key = KeyParser.StringToKey(Key);
            Shortcut.Modifiers = ConvertToModifierKeys();
            Shortcut.Script = Document.Text;
            Shortcut.CompiledScript = compiled_script;
            WeakReferenceMessenger.Default.Send(new BackMessage());
        }
        else
        {
            Output = msg;
        }

        Shortcut = null;
        Output = string.Empty;
    }

    [RelayCommand]
    private void Cancel()
    {
        WeakReferenceMessenger.Default.Send(new BackMessage());

        Shortcut = null;
        Output = string.Empty;
    }
}
