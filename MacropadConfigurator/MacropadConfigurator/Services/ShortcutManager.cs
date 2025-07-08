using System.Collections.ObjectModel;
using System.IO;
using System.Text.Json;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using MacropadConfigurator.Extensions;
using MacropadConfigurator.Models;
using NLog;

namespace MacropadConfigurator.Services;

public partial class ShortcutManager : ObservableObject
{
    private static readonly Logger logger = LogManager.GetCurrentClassLogger();

    private readonly CompilerService compilerService;

    [ObservableProperty]
    private bool isReady = false;

    public ObservableCollection<Shortcut> Shortcuts { get; private set; } = [];

    public ShortcutManager(CompilerService compilerService)
    {
        this.compilerService = compilerService;
    }

    public void AddDefaults()
    {
        Shortcuts.Add(new Shortcut(Key.F13, "Open Apps", "Open", "ShowMessageBox(\"Opening Apps\");"));
        Shortcuts.Add(new Shortcut(Key.F14, "Position Apps", "Pos", "ShowMessageBox(\"Positioning Apps\");"));
        Shortcuts.Add(new Shortcut(Key.F15, "Show Configurator", "GUI", ""));
        Shortcuts.Add(new Shortcut(Key.X, ModifierKeys.Alt | ModifierKeys.Shift | ModifierKeys.Control, "Exit", "Exit", ""));
    }

    public void Initialize()
    {
        foreach (var shortcut in Shortcuts)
        {
            (var result, var msg, var script) = compilerService.CompileScript(shortcut.Script);
         
            if (result && script != null)
                shortcut.CompiledScript = script;
            else
                logger.Error($"Error compiling script for shortcut {shortcut.Name}: {msg}");
        }
    }

    public Shortcut Create()
    {
        var shortcut = new Shortcut(Key.A, "New Shortcut", "NA", "");
        Shortcuts.Add(shortcut);
        return shortcut;
    }

    public void Delete(Shortcut shortcut)
    {
        logger.Info($"Deleting shortcut: {shortcut.Name}");
        Shortcuts.Remove(shortcut);
    }

    public void Load(string path)
    {
        logger.Info("Loading shortcuts");

        try
        {
            string jsonString = File.ReadAllText(path);
            var temp = JsonSerializer.Deserialize<List<Shortcut>>(jsonString) ?? [];
            Shortcuts = temp.ToObservableCollection();
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
            string jsonString = JsonSerializer.Serialize(Shortcuts, options);
            File.WriteAllText(path, jsonString);
        }
        catch (Exception ex)
        {
            logger.Error($"Error saving shortcuts: {ex.Message}");
        }
    }
}
