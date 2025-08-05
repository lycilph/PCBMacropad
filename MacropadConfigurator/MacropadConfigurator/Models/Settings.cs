namespace MacropadConfigurator.Models;

public class Settings
{
    public string BaseColorScheme { get; set; } = string.Empty;
    public string ColorScheme { get; set; } = string.Empty;
    public bool HideOnClose { get; set; } = false;
    public bool RunOnStartup { get; set; } = false;
}
