namespace MacropadConfigurator.Messages;

public sealed class LogMessage(string text)
{
    public string Text { get; set; } = text;
}
