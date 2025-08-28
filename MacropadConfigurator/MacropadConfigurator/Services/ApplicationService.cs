using System.Diagnostics;
using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using Hardcodet.Wpf.TaskbarNotification;
using MacropadConfigurator.Messages;
using NLog;

namespace MacropadConfigurator.Services;

public class ApplicationService 
    : ObservableRecipient, 
      IRecipient<StartupMessage>, 
      IRecipient<DataChangedMessage>,
      IRecipient<ShowToastMessage>
{
    private static readonly Logger logger = LogManager.GetCurrentClassLogger();

    private readonly string folderName = "MacropadConfigurator";
    private readonly string applicationLink = "MacropadConfigurator.lnk";
    private readonly string settingsFile = "settings.json";
    private readonly string shortcutsFile = "shortcuts.json";

    private readonly SettingsService settingsService;
    private readonly ScriptService scriptService;
    private readonly ConfigurationService configurationService;

    private readonly KeyboardHook keyboardHook;

    private TaskbarIcon? notifyIcon = null;

    public ApplicationService(SettingsService settingsService,
                              ScriptService scriptService,
                              ConfigurationService configurationService)
    {
        this.settingsService = settingsService;
        this.scriptService = scriptService;
        this.configurationService = configurationService;

        keyboardHook = new KeyboardHook();

        IsActive = true;
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

    public void Start(TaskbarIcon notifyIcon)
    {
        this.notifyIcon = notifyIcon;

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
        SaveData();

        keyboardHook.ShortcutPressed -= KeyboardHook_ShortcutPressed;
        keyboardHook.Dispose();
    }

    private void SaveData()
    {
        settingsService.Save(GetPath(settingsFile));
        configurationService.Save(GetPath(shortcutsFile));
    }

    private void KeyboardHook_ShortcutPressed(System.Windows.Input.Key arg1, System.Windows.Input.ModifierKeys arg2)
    {
        configurationService.Current.Layers
            .Where(l => l.IsEnabled)
            .SelectMany(l => l.Shortcuts)
            .Where(s => s.Key == arg1 && s.Modifiers == arg2 & !string.IsNullOrEmpty(s.Script))
            .ToList()
            .ForEach(async s =>
            {
                logger.Trace($"[{arg1} - {arg2}] was pressed - Triggering shortcut [{s.Text}]");
                await scriptService.RunOnUIThreadAsync(s.Script);
            });
    }

    public void Receive(StartupMessage message)
    {
        if (message.RunOnStartup)
            CreateShortcut();
        else
            RemoveShortcut();
    }

    public void Receive(DataChangedMessage message)
    {
        SaveData();
    }

    public void Receive(ShowToastMessage message)
    {
        notifyIcon?.ShowBalloonTip("Information", message.Text, BalloonIcon.Info);
    }
    
    private void CreateShortcut()
    {
        var startupFolder = Environment.GetFolderPath(Environment.SpecialFolder.Startup);
        var shortcutPath = Path.Combine(startupFolder, "MacropadConfigurator.lnk");
        var exePath = Process.GetCurrentProcess()?.MainModule?.FileName;

        if (File.Exists(shortcutPath)) return;

        var shell_type = Type.GetTypeFromProgID("WScript.Shell");
        if (shell_type != null)
        {
            dynamic? shell = Activator.CreateInstance(shell_type);
            if (shell != null)
            {
                var shortcut = shell.CreateShortcut(shortcutPath);
                shortcut.TargetPath = exePath;
                shortcut.WorkingDirectory = Path.GetDirectoryName(exePath);
                shortcut.Description = "Launches Macropad Configurator on startup";
                shortcut.Save();
            }
        }
    }

    private void RemoveShortcut()
    {
        var startupFolder = Environment.GetFolderPath(Environment.SpecialFolder.Startup);
        var shortcutPath = Path.Combine(startupFolder, "MacropadConfigurator.lnk");

        if (File.Exists(shortcutPath))
            File.Delete(shortcutPath);
    }
}
