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
    private ShellWindow mainWindow = null!;

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
        services.AddSingleton<ApplicationManager>();
        services.AddSingleton<ShortcutManager>();
        services.AddSingleton<SettingsService>();
        services.AddSingleton<CompilerService>();

        // Register view models
        services.AddTransient<ShellViewModel>();
        services.AddTransient<MainViewModel>();
        services.AddTransient<SettingsViewModel>();
        services.AddTransient<ConfigurationViewModel>();
        services.AddTransient<ShortcutsViewModel>();
        services.AddTransient<EditShortcutViewModel>();

        return services.BuildServiceProvider();
    }

    private void ApplicationStartup(object sender, StartupEventArgs e)
    {
        logger.Info("Application starting...");

        // Initialize application
        var applicationManager = Services.GetRequiredService<ApplicationManager>();
        applicationManager.Load();

        // Initialize the NotifyIcon
        notifyIcon = (TaskbarIcon)FindResource("NotifyIcon");
        notifyIcon.DataContext = new NotifyIconViewModel(this);

        // Initialize main window
        var vm = Services.GetRequiredService<ShellViewModel>();
        mainWindow = new ShellWindow { DataContext = vm };

        ShowWindow();
    }

    private void ApplicationExit(object sender, ExitEventArgs e)
    {
        logger.Info("Application exiting...");

        var applicationManager = Services.GetRequiredService<ApplicationManager>();
        applicationManager.Save();

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
