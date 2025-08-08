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
    private readonly ScriptService scriptService;

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

    public EditShortcutViewModel(ScriptService scriptService)
    {
        this.scriptService = scriptService;
    }

    public void Set(Shortcut shortcut)
    {
        this.shortcut = shortcut;

        Text = shortcut.Text;
        Key = shortcut.Key.ToString();
        Control = shortcut.Modifiers.HasFlag(ModifierKeys.Control);
        Shift = shortcut.Modifiers.HasFlag(ModifierKeys.Shift);
        Alt = shortcut.Modifiers.HasFlag(ModifierKeys.Alt);
        Document = new TextDocument(shortcut.Script);
    }

    [RelayCommand]
    private async Task CompileAsync()
    {
        var result = await scriptService.CompileAsync(Document.Text);
        Output = result.Success ? "Success" : result.Message;
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        if (shortcut == null) return;
        
        var parsed_key = KeyParser.StringToKey(Key);
        var result = await scriptService.CompileAsync(Document.Text);

        if (Key != System.Windows.Input.Key.None.ToString() && parsed_key == System.Windows.Input.Key.None)
        {
            // Error in parsing key
            Output = $"Couldn't parse the key [{Key}]";
        }
        else if (!string.IsNullOrWhiteSpace(Document.Text) && !result.Success)
        {
            // Error in parsing script
            Output = result.Message;
        }
        else
        {
            shortcut.Text = Text;
            shortcut.Key = KeyParser.StringToKey(Key);
            shortcut.Modifiers = (Control ? ModifierKeys.Control : ModifierKeys.None) |
                                 (Shift ? ModifierKeys.Shift : ModifierKeys.None) |
                                 (Alt ? ModifierKeys.Alt : ModifierKeys.None);

            shortcut.Script = Document.Text;
            shortcut.CompiledScript = result.Script;

            shortcut = null;
            Output = string.Empty;

            WeakReferenceMessenger.Default.Send(new NavigateToMainMessage());
        }
    }

    [RelayCommand]
    private void Cancel()
    {
        WeakReferenceMessenger.Default.Send(new NavigateToMainMessage());

        shortcut = null;
        Output = string.Empty;
    }
}
