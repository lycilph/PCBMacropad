using MacropadConfigurator.Models;
using NLog;

namespace MacropadConfigurator.Services;

public class ShortcutManager
{
    private static readonly Logger logger = LogManager.GetCurrentClassLogger();

    public List<Shortcut> Shortcuts { get; } = [];

    public void Load(string path)
    {
        logger.Info("Loading shortcuts");
    }

    public void Save(string path)
    {
        logger.Info("Saving shortcuts");
    }

    //public async Task<List<Shortcut>> LoadShortcutsAsync()
    //{
    //    if (!File.Exists(_filePath)) return [];

    //    try
    //    {
    //        string jsonString = await File.ReadAllTextAsync(_filePath);
    //        return JsonSerializer.Deserialize<List<Shortcut>>(jsonString) ?? [];
    //    }
    //    catch (Exception ex)
    //    {
    //        MessageBox.Show($"Failed to load shortcuts from file:\n{_filePath}\n\nError: {ex.Message}", "Load Error", MessageBoxButton.OK, MessageBoxImage.Error);

    //        return []; // Return an empty list on failure to prevent a crash.
    //    }
    //}

    //public async Task SaveShortcutsAsync(IEnumerable<Shortcut> shortcuts)
    //{
    //    try
    //    {
    //        // Configure the serializer to write indented JSON for readability.
    //        var options = new JsonSerializerOptions { WriteIndented = true };
    //        string jsonString = JsonSerializer.Serialize(shortcuts, options);
    //        await File.WriteAllTextAsync(_filePath, jsonString);
    //    }
    //    catch (Exception ex)
    //    {
    //        MessageBox.Show($"Failed to save shortcuts to file:\n{_filePath}\n\nError: {ex.Message}", "Save Error", MessageBoxButton.OK, MessageBoxImage.Error);
    //    }
    //}
}
