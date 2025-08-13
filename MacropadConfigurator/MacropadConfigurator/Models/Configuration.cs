using MacropadConfigurator.DTO;

namespace MacropadConfigurator.Models;

public class Configuration
{
    public MasterScript MasterScript { get; set; } = new MasterScript();
    public List<Layer> Layers { get; set; } = [];

    public void Update(MacropadConfigurationDTO dto)
    {
        for (int i = 0; i < Layers.Count; i++)
            Layers[i].Update(dto.layers[i]);
    }

    public MacropadConfigurationDTO ToDto()
    {
        return new MacropadConfigurationDTO
        {
            layers = Layers.Select(l => l.ToDto()).ToArray()
        };
    }

    public void Reset()
    {
        Layers.ForEach(l => l.Reset());
    }
}
