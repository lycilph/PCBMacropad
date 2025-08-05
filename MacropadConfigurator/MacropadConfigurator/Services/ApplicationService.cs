using System.IO;

namespace MacropadConfigurator.Services;

public class ApplicationService
{
    private readonly string folderName = "MacropadConfigurator";
    private readonly string settingsFile = "settings.json";

    private readonly SettingsService settingsService;

    public ApplicationService(SettingsService settingsService)
    {
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
    }

    public void Save()
    {
        settingsService.Save(GetPath(settingsFile));
    }
}
