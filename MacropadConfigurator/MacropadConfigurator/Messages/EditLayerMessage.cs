using MacropadConfigurator.Models;

namespace MacropadConfigurator.Messages;

public sealed class EditLayerMessage(Layer layer)
{
    public Layer Layer { get; set; } = layer;
}
