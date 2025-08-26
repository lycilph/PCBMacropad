namespace MacropadConfigurator.Messages;

public sealed class ShowToastMessage(string text)
{
    public string Text { get; private set; } = text;
}
