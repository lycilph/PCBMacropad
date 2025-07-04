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

    public void Load(string path)
    {
        logger.Info("Loading settings");

        try
        {
            if (File.Exists(path))
            {
                string json = File.ReadAllText(path);
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
            logger.Error($"Error loading settings: {ex.Message}");
        }
    }

    public void Save(string path)
    {
        logger.Info("Saving settings");

        var settings = new Settings
        {
            BaseColorScheme = GetBaseColorScheme(),
            ColorScheme = GetColorScheme(),
        };

        try
        {
            // Configure the serializer to write indented JSON for readability.
            var options = new JsonSerializerOptions { WriteIndented = true };
            string json = JsonSerializer.Serialize(settings, options);
            File.WriteAllText(path, json);
        }
        catch (Exception ex)
        {
            logger.Error($"Error saving settings: {ex.Message}");
        }
    }
}
