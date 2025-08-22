using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using ICSharpCode.AvalonEdit.Document;
using MacropadConfigurator.Messages;
using MacropadConfigurator.Services;

namespace MacropadConfigurator.ViewModels;

public partial class EditMasterScriptViewModel : ObservableObject
{
    private readonly ConfigurationService configurationService;
    private readonly ScriptService scriptService;

    [ObservableProperty]
    private TextDocument document = new();

    [ObservableProperty]
    private string output = string.Empty;

    public EditMasterScriptViewModel(ConfigurationService configurationService, ScriptService scriptService)
    {
        this.configurationService = configurationService;
        this.scriptService = scriptService;
    }

    public void Activate()
    {
        Document = new TextDocument(configurationService.Current.MasterScript.Script);
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

            configurationService.Current.MasterScript.Script = Document.Text;
            await scriptService.UpdateStateAsync(configurationService.Current.MasterScript.Script);

            Output = string.Empty;

            WeakReferenceMessenger.Default.Send(new DataChangedMessage());
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
