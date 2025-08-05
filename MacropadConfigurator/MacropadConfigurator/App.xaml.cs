using Hardcodet.Wpf.TaskbarNotification;
using MacropadConfigurator.Services;
using MacropadConfigurator.ViewModels;
using MacropadConfigurator.Views;
using MahApps.Metro.Controls.Dialogs;
using Microsoft.Extensions.DependencyInjection;
using NLog;
using System.Windows;

namespace MacropadConfigurator;

public partial class App : Application
{
    private static readonly Logger logger = LogManager.GetCurrentClassLogger();
    public new static App Current => (App)Application.Current;
    public IServiceProvider Services { get; }

    private TaskbarIcon notifyIcon = null!;
    private ShellWindow window = null!;

    public App()
    {
        logger.Info("Application starting...");

        Services = ConfigureServices();

        InitializeComponent();
    }

    private static IServiceProvider ConfigureServices()
    {
        var services = new ServiceCollection();

        // Framework stuff
        services.AddSingleton(DialogCoordinator.Instance);

        // Register singletons
        services.AddSingleton<ApplicationService>();
        services.AddSingleton<OverlayService>();
        services.AddSingleton<SettingsService>();

        // Register view models
        services.AddTransient<ShellViewModel>();
        services.AddTransient<MainViewModel>();
        services.AddTransient<SettingsViewModel>();

        return services.BuildServiceProvider();
    }

    private void ApplicationStartup(object sender, StartupEventArgs e)
    {
        logger.Info("Application starting...");

        // Initialize application
        var applicationManager = Services.GetRequiredService<ApplicationService>();
        applicationManager.Load();

        // Initialize the NotifyIcon
        notifyIcon = (TaskbarIcon)FindResource("NotifyIcon");
        notifyIcon.DataContext = new NotifyIconViewModel(this);

        // Initialize main window
        var vm = Services.GetRequiredService<ShellViewModel>();
        window = new ShellWindow { DataContext = vm };

        ShowWindow();
    }

    private void ApplicationExit(object sender, ExitEventArgs e)
    {
        logger.Info("Application exiting...");

        var applicationManager = Services.GetRequiredService<ApplicationService>();
        applicationManager.Save();

#pragma warning disable CA1416 // Validate platform compatibility
        notifyIcon.Dispose();
#pragma warning restore CA1416 // Validate platform compatibility
    }

    public void ShowWindow()
    {
        window.Show();
        window.Activate();
    }

    public void HideWindow()
    {
        window.Hide();
    }
}
