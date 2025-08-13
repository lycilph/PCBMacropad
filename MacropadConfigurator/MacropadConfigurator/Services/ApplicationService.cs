using System.IO;
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
    private readonly ConfigurationService configurationService;

    private readonly KeyboardHook keyboardHook;

    public ApplicationService(SettingsService settingsService, ScriptService scriptService, ConfigurationService configurationService)
    {
        this.settingsService = settingsService;
        this.scriptService = scriptService;
        this.configurationService = configurationService;

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

    public async Task InitializeScriptStateAsync()
    {
        logger.Info("Compiling master script");
        await scriptService.UpdateStateAsync(configurationService.Current.MasterScript.Script);
    }

    public void Start()
    {
        settingsService.Load(GetPath(settingsFile));
        configurationService.Load(GetPath(shortcutsFile));

        if (configurationService.Current.Layers.Count == 0)
        {
            logger.Info("No shortcuts found, adding default layers");
            configurationService.CreateDefaultLayers();
        }

        keyboardHook.ShortcutPressed += KeyboardHook_ShortcutPressed;
    }

    public void Stop()
    {
        settingsService.Save(GetPath(settingsFile));
        configurationService.Save(GetPath(shortcutsFile));

        keyboardHook.ShortcutPressed -= KeyboardHook_ShortcutPressed;
        keyboardHook.Dispose();
    }


    private void KeyboardHook_ShortcutPressed(System.Windows.Input.Key arg1, System.Windows.Input.ModifierKeys arg2)
    {
        configurationService.Current.Layers
            .Where(l => l.IsEnabled)
            .SelectMany(l => l.Shortcuts)
            .Where(s => s.Key == arg1 && s.Modifiers == arg2)
            .ToList()
            .ForEach(async s =>
            {
                logger.Trace($"[{arg1} - {arg2}] was pressed - Triggering shortcut [{s.Text}]");
                await scriptService.RunAsync(s.Script);
            });
    }
}
