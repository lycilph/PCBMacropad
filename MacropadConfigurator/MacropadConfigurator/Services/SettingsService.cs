using System.IO;
using System.Text.Json;
using ControlzEx.Theming;
using MacropadConfigurator.Models;
using NLog;

namespace MacropadConfigurator.Services;

public class SettingsService
{
    private static readonly Logger logger = LogManager.GetCurrentClassLogger();

    public static readonly string Light = "Light";
    public static readonly string Dark = "Dark";
    public static readonly string DefaultColorScheme = "Steel";

    public readonly string settings_file = "settings.json";

    public List<Theme> Themes { get; private set; }

    public SettingsService()
    {
        logger.Info("Loading themes");

        Themes = ThemeManager.Current.Themes.Where(t => t.BaseColorScheme == Light).ToList();
    }

    public string GetBaseColorScheme() => ThemeManager.Current.DetectTheme()?.BaseColorScheme ?? Light;

    public string GetColorScheme() => ThemeManager.Current.DetectTheme()?.ColorScheme ?? DefaultColorScheme;

    public void SetBaseColorScheme(bool light)
    {
        var base_color_scheme = light ? Light : Dark;
        ThemeManager.Current.ChangeThemeBaseColor(App.Current, base_color_scheme);
    }

    public void SetColorScheme(string color_scheme)
    {
        ThemeManager.Current.ChangeThemeColorScheme(App.Current, color_scheme);
    }

    public string GetPath()
    {
        string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        string appFolderPath = Path.Combine(appDataPath, "MacropadConfigurator");

        // Ensure the directory exists
        Directory.CreateDirectory(appFolderPath);

        return Path.Combine(appFolderPath, settings_file);
    }

    public void Load()
    {
        logger.Info("Loading settings");

        try
        {
            if (File.Exists(GetPath()))
            {
                string json = File.ReadAllText(GetPath());
                var settings = JsonSerializer.Deserialize<Settings>(json);

                if (settings != null)
                {
                    // Apply the saved theme
                    ThemeManager.Current.ChangeTheme(App.Current, settings.BaseColorScheme, settings.ColorScheme);
                }
            }
        }
        catch (Exception ex)
        {
            logger.Error($"Error loading theme settings: {ex.Message}");
        }
    }

    public void Save()
    {
        logger.Info("Saving settings");

        var settings = new Settings
        {
            BaseColorScheme = GetBaseColorScheme(),
            ColorScheme = GetColorScheme(),
        };

        try
        {
            string json = JsonSerializer.Serialize(settings);
            File.WriteAllText(GetPath(), json);
        }
        catch (Exception ex)
        {
            logger.Error($"Error saving theme settings: {ex.Message}");
        }
    }
}
