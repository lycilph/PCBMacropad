using System.IO;
using System.Text.Json;
using MacropadConfigurator.DTO;
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
    private readonly ScriptService scriptService;

    private readonly KeyboardHook keyboardHook;

    public List<Layer> Layers { get; private set; } = [];

    public ApplicationService(SettingsService settingsService, ScriptService scriptService)
    {
        this.settingsService = settingsService;
        this.scriptService = scriptService;

        keyboardHook = new KeyboardHook();
    }

    public string GetPath(string filename)
    {
        string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        string appFolderPath = Path.Combine(appDataPath, folderName);

        // Ensure the directory exists
        Directory.CreateDirectory(appFolderPath);

        return Path.Combine(appFolderPath, filename);
    }

    public void InitializeScripts()
    {
        logger.Info("Compiling scripts");

        Layers
           .SelectMany(l => l.Shortcuts)
           .Where(s => !string.IsNullOrWhiteSpace(s.Script))
           .ToList()
           .ForEach(async s => 
           {
               var result = await scriptService.CompileAsync(s.Script);
               s.CompiledScript = result.Script;
           });
    }

    public void UpdateShortcuts(MacropadConfigurationDTO config)
    {
        for (int i = 0; i < Layers.Count; i++)
        {
            Layers[i].Update(config.layers[i]);
        }
    }

    public void ResetShortcuts()
    {
        logger.Info("Resetting shortcuts");
        Layers.ForEach(l => l.Reset());
    }

    public void Start()
    {
        settingsService.Load(GetPath(settingsFile));
        LoadShortcuts(GetPath(shortcutsFile));

        if (Layers.Count == 0)
        {
            logger.Info("No shortcuts found, adding default layers");
            AddDefaultLayers();
        }

        keyboardHook.ShortcutPressed += KeyboardHook_ShortcutPressed;
    }

    public void Stop()
    {
        settingsService.Save(GetPath(settingsFile));
        SaveShortcuts(GetPath(shortcutsFile));

        keyboardHook.ShortcutPressed -= KeyboardHook_ShortcutPressed;
        keyboardHook.Dispose();
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

    private void AddDefaultLayers()
    {
        Layers =
        [
            new Layer { Name = "Layer 1" },
            new Layer { Name = "Layer 2" },
            new Layer { Name = "Layer 3" }
        ];
    }

    private void KeyboardHook_ShortcutPressed(System.Windows.Input.Key arg1, System.Windows.Input.ModifierKeys arg2)
    {
        Layers
            .Where(l => l.IsEnabled)
            .SelectMany(l => l.Shortcuts)
            .Where(s => s.Key == arg1 && s.Modifiers == arg2)
            .ToList()
            .ForEach(s => 
            {
                logger.Trace($"[{arg1} - {arg2}] was pressed - Triggering shortcut [{s.Text}]");
                scriptService.ExecuteScriptAsync(s.CompiledScript);
            });
    }
}
