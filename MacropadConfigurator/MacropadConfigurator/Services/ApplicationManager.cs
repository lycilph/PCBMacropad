using System.IO;

namespace MacropadConfigurator.Services;

public class ApplicationManager
{
    private readonly ShortcutManager shortcutManager;
    private readonly SettingsService settingsService;

    private string folderName = "MacropadConfigurator";
    private string settingsFile = "settings.json";
    private string shortcutsFile = "shortcuts.json";

    public ApplicationManager(ShortcutManager shortcutManager, SettingsService settingsService)
    {
        this.shortcutManager = shortcutManager;
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
        shortcutManager.Load(GetPath(shortcutsFile));

        if (shortcutManager.Shortcuts.Count == 0)
            shortcutManager.AddDefaults();
    }

    public void Save()
    {
        settingsService.Save(GetPath(settingsFile));
        shortcutManager.Save(GetPath(shortcutsFile));
    }
}
