namespace MacropadConfigurator.Services;

public class CommunicationService
{
    public event EventHandler? ConfigurationLoaded;
    public event EventHandler? ConfigurationSaved;
    public event EventHandler? ConfigurationReset;

    public void LoadConfiguration()
    {
        Task.Delay(2000)
            .ContinueWith(_ => OnConfigurationLoaded());
    }

    public void SaveConfiguration()
    {
        Task.Delay(2000)
            .ContinueWith(_ => OnConfigurationSaved());
    }

    public void ResetConfiguration()
    {
        Task.Delay(2000)
            .ContinueWith(_ => OnConfigurationReset());
    }

    public void OnConfigurationLoaded() => ConfigurationLoaded?.Invoke(this, EventArgs.Empty);
    public void OnConfigurationSaved() => ConfigurationSaved?.Invoke(this, EventArgs.Empty);
    public void OnConfigurationReset() => ConfigurationReset?.Invoke(this, EventArgs.Empty);
}
