using System.Windows;
using MacropadConfigurator.Services;
using MacropadConfigurator.ViewModels;
using MacropadConfigurator.Views;
using Microsoft.Extensions.DependencyInjection;
using NLog;

namespace MacropadConfigurator;

public partial class App : Application
{
    private static readonly Logger logger = LogManager.GetCurrentClassLogger();
    public IServiceProvider Services { get; }

    public App()
    {
        logger.Info("Application starting...");

        Services = ConfigureServices();

        InitializeComponent();
    }

    private static IServiceProvider ConfigureServices()
    {
        var services = new ServiceCollection();

        // Register singletons
        services.AddSingleton<SettingsService>();

        // These view models are singletons because they are used by multiple view models
        services.AddSingleton<SettingsViewModel>();

        // Register view models
        services.AddTransient<MainViewModel>();

        return services.BuildServiceProvider();
    }

    private void ApplicationStartup(object sender, StartupEventArgs e)
    {
        logger.Info("Application starting...");

        var settings = Services.GetRequiredService<SettingsService>();
        settings.Load();

        var vm = Services.GetRequiredService<MainViewModel>();
        var win = new MainWindow { DataContext = vm };
        win.Show();
    }

    private void ApplicationExit(object sender, ExitEventArgs e)
    {
        logger.Info("Application exiting...");

        var settings = Services.GetRequiredService<SettingsService>();
        settings.Save();
    }
}
