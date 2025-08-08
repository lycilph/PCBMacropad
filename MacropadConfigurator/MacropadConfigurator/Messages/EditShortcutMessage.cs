using MacropadConfigurator.Models;

namespace MacropadConfigurator.Messages;

public sealed class EditShortcutMessage(Shortcut shortcut)
{
    public Shortcut Shortcut { get; set; } = shortcut;
}
