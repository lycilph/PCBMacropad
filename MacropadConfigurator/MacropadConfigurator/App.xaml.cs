using System.Windows;
using Hardcodet.Wpf.TaskbarNotification;
using MacropadConfigurator.Services;
using MacropadConfigurator.ViewModels;
using MacropadConfigurator.Views;
using Microsoft.Extensions.DependencyInjection;
using NLog;

namespace MacropadConfigurator;

public partial class App : Application
{
    private static readonly Logger logger = LogManager.GetCurrentClassLogger();
    public new static App Current => (App)Application.Current;
    public IServiceProvider Services { get; }

    private TaskbarIcon notifyIcon = null!;
    private MainWindow mainWindow = null!;

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

        // Register view models
        services.AddTransient<MainViewModel>();
        services.AddTransient<SettingsViewModel>();
        services.AddTransient<ConfigurationViewModel>();
        services.AddTransient<ShortcutsViewModel>();

        return services.BuildServiceProvider();
    }

    private void ApplicationStartup(object sender, StartupEventArgs e)
    {
        logger.Info("Application starting...");

        // Initialize settings
        var settings = Services.GetRequiredService<SettingsService>();
        settings.Load();

        // Initialize the NotifyIcon
        notifyIcon = (TaskbarIcon)FindResource("NotifyIcon");
        notifyIcon.DataContext = new NotifyIconViewModel(this);

        // Initialize main window
        var vm = Services.GetRequiredService<MainViewModel>();
        mainWindow = new MainWindow { DataContext = vm };

        ShowWindow();
    }

    private void ApplicationExit(object sender, ExitEventArgs e)
    {
        logger.Info("Application exiting...");

        var settings = Services.GetRequiredService<SettingsService>();
        settings.Save();

        notifyIcon.Dispose();
    }

    public void ShowWindow()
    {
        mainWindow.Show();
        mainWindow.Activate();
    }

    public void HideWindow()
    {
        mainWindow.Hide();
    }
}
