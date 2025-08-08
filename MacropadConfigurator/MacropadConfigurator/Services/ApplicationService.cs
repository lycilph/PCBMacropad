using System.IO;
using System.Text.Json;
using System.Windows.Forms;
using System.Windows.Input;
using MacropadConfigurator.Models;
using NLog;

namespace MacropadConfigurator.Services;

public class ApplicationService
{
    private static readonly Logger logger = LogManager.GetCurrentClassLogger();

    private readonly string folderName = "MacropadConfigurator";
    private readonly string settingsFile = "settings.json";
    private readonly string shortcutsFile = "shortcuts.json";

    private readonly SettingsService settingsService;

    public List<Layer> Layers { get; private set; } = [];

    public ApplicationService(SettingsService settingsService)
    {
        this.settingsService = settingsService;
    }
    
    public string GetPath(string filename)
    {
        string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        string appFolderPath = Path.Combine(appDataPath, folderName);

        // Ensure the directory exists
        Directory.CreateDirectory(appFolderPath);

        return Path.Combine(appFolderPath, filename);
    }

    public void Load()
    {
        settingsService.Load(GetPath(settingsFile));
        LoadShortcuts(GetPath(shortcutsFile));

        if (Layers.Count == 0)
            CreateDefaultLayers();
    }

    public void Save()
    {
        settingsService.Save(GetPath(settingsFile));
        SaveShortcuts(GetPath(shortcutsFile));
    }

    private void LoadShortcuts(string path)
    {
        logger.Info("Loading shortcuts");

        try
        {
            string jsonString = File.ReadAllText(path);
            Layers = JsonSerializer.Deserialize<List<Layer>>(jsonString) ?? [];
        }
        catch (Exception ex)
        {
            logger.Error($"Error loading shortcuts: {ex.Message}");
        }
    }

    private void SaveShortcuts(string path)
    {
        logger.Info("Saving shortcuts");

        try
        {
            // Configure the serializer to write indented JSON for readability.
            var options = new JsonSerializerOptions { WriteIndented = true };
            string jsonString = JsonSerializer.Serialize(Layers, options);
            File.WriteAllText(path, jsonString);
        }
        catch (Exception ex)
        {
            logger.Error($"Error saving shortcuts: {ex.Message}");
        }
    }

    private void CreateDefaultLayers()
    {
        logger.Info("Adding default layers");

        Layers =
        [
            new Layer() { Name = "Layer 1", IsEnabled = true },
            new Layer() { Name = "Layer 2", IsEnabled = true },
            new Layer() { Name = "Layer 3", IsEnabled = true }
        ];
    }
}
