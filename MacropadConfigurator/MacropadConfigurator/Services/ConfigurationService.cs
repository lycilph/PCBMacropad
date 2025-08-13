
using System.IO;
using System.Text.Json;
using MacropadConfigurator.Models;
using NLog;

namespace MacropadConfigurator.Services;

public class ConfigurationService
{
    private static readonly Logger logger = LogManager.GetCurrentClassLogger();

    public Configuration Current { get; private set; } = new();

    public void CreateDefaultLayers()
    {
        Current.Layers =
        [
            new Layer { Name = "Layer 1" },
            new Layer { Name = "Layer 2" },
            new Layer { Name = "Layer 3" }
        ];
    }

    public void Load(string path)
    {
        logger.Info("Loading shortcuts");

        try
        {
            string jsonString = File.ReadAllText(path);
            Current = JsonSerializer.Deserialize<Configuration>(jsonString) ?? new Configuration();
        }
        catch (Exception ex)
        {
            logger.Error($"Error loading shortcuts: {ex.Message}");
        }
    }

    public void Save(string path)
    {
        logger.Info("Saving shortcuts");

        try
        {
            // Configure the serializer to write indented JSON for readability.
            var options = new JsonSerializerOptions { WriteIndented = true };
            string jsonString = JsonSerializer.Serialize(Current, options);
            File.WriteAllText(path, jsonString);
        }
        catch (Exception ex)
        {
            logger.Error($"Error saving shortcuts: {ex.Message}");
        }
    }
}
