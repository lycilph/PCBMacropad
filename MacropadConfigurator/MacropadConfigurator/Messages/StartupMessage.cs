namespace MacropadConfigurator.Messages;

public sealed class StartupMessage(bool runOnStart)
{
    public bool RunOnStartup { get; private set; } = runOnStart;
}
