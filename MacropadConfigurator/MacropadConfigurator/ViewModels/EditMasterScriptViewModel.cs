using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using ICSharpCode.AvalonEdit.Document;
using MacropadConfigurator.Messages;
using MacropadConfigurator.Services;

namespace MacropadConfigurator.ViewModels;

public partial class EditMasterScriptViewModel : ObservableObject
{
    private readonly ApplicationService applicationService;
    private readonly ScriptService scriptService;

    [ObservableProperty]
    private TextDocument document = new();

    [ObservableProperty]
    private string output = string.Empty;

    public EditMasterScriptViewModel(ApplicationService applicationService, ScriptService scriptService)
    {
        this.applicationService = applicationService;
        this.scriptService = scriptService;
    }

    public void Activate()
    {
        Document = new TextDocument(applicationService.Configuration.MasterScript.Script);
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
        var result = await scriptService.CompileAsync(Document.Text);

        if (!string.IsNullOrWhiteSpace(Document.Text) && !result.Success)
        {
            // Error in parsing script
            
            Output = result.Message;
        }
        else
        {
            // Script compiled successfully

            var ms = applicationService.Configuration.MasterScript;
            ms.Script = Document.Text;
            ms.CompiledScript = result.Script;

            Output = string.Empty;
            WeakReferenceMessenger.Default.Send(new NavigateToMainMessage());
        }
    }

    [RelayCommand]
    private void Cancel()
    {
        WeakReferenceMessenger.Default.Send(new NavigateToMainMessage());
        Output = string.Empty;
    }
}
