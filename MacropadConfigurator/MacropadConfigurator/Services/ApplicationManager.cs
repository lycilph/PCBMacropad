using System.IO;
using System.Windows.Input;

namespace MacropadConfigurator.Services;

public class ApplicationManager
{
    private readonly ShortcutManager shortcutManager;
    private readonly SettingsService settingsService;
    private readonly CompilerService compilerService;

    private readonly KeyboardHook keyboardHook;

    private readonly string folderName = "MacropadConfigurator";
    private readonly string settingsFile = "settings.json";
    private readonly string shortcutsFile = "shortcuts.json";

    public ApplicationManager(ShortcutManager shortcutManager, SettingsService settingsService, CompilerService compilerService)
    {
        this.shortcutManager = shortcutManager;
        this.settingsService = settingsService;
        this.compilerService = compilerService;

        // Initialize the keyboard hook
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

    private void OnShortcutPressedAsync(Key key, ModifierKeys modifiers)
    {
        // Check if the pressed combination matches any of our registered shortcuts
        foreach (var shortcut in shortcutManager.Shortcuts)
        {
            if (shortcut.Key == key && shortcut.Modifiers == modifiers)
            {
                compilerService.ExecuteScript(shortcut.CompiledScript);
            }
        }
    }

    // Should be called after load (so the shortcut scripts have been loaded and are ready to be compiled)
    public async void InitializeAsync()
    {
        await shortcutManager.InitializeAsync();
        keyboardHook.ShortcutPressed += OnShortcutPressedAsync;
    }

    public void Cleanup()
    {
        keyboardHook.ShortcutPressed -= OnShortcutPressedAsync;
        keyboardHook.Dispose();
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
