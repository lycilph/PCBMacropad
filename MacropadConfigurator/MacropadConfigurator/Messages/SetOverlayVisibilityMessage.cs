namespace MacropadConfigurator.Messages;

public sealed class SetOverlayVisibilityMessage(bool visible)
{
    public bool Visible { get; } = visible;
}
