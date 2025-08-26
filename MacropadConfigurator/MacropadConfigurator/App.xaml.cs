using Hardcodet.Wpf.TaskbarNotification;
using MacropadConfigurator.Misc;
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

    private static Mutex? singleInstanceMutex;

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
        services.AddSingleton<CommunicationService>();
        services.AddSingleton<ScriptService>();
        services.AddSingleton<ConfigurationService>();

        // Register view models
        services.AddTransient<ShellViewModel>();
        services.AddTransient<MainViewModel>();
        services.AddTransient<SettingsViewModel>();
        services.AddTransient<EditShortcutViewModel>();
        services.AddTransient<EditMasterScriptViewModel>();

        return services.BuildServiceProvider();
    }

    private void ApplicationStartup(object sender, StartupEventArgs e)
    {
        logger.Info("Application starting...");

        CheckForSingleInstance();

        // Initialize the NotifyIcon
        notifyIcon = (TaskbarIcon)FindResource("NotifyIcon");
        notifyIcon.DataContext = new NotifyIconViewModel(this);

        // Initialize application
        var applicationManager = Services.GetRequiredService<ApplicationService>();
        applicationManager.Start(notifyIcon);

        // Initialize main window
        var vm = Services.GetRequiredService<ShellViewModel>();
        window = new ShellWindow { DataContext = vm };

        // Check if this is started automatically (and if show start minimized)
        var settings = Services.GetRequiredService<SettingsService>();
        ShowWindow(settings.RunOnStartup && !DebugHelper.IsDebug());
    }

    private void ApplicationExit(object sender, ExitEventArgs e)
    {
        logger.Info("Application exiting...");

        var applicationManager = Services.GetRequiredService<ApplicationService>();
        applicationManager.Stop();

#pragma warning disable CA1416 // Validate platform compatibility
        notifyIcon.Dispose();
#pragma warning restore CA1416 // Validate platform compatibility
    }

    public void ShowWindow(bool show_minimized = false)
    {
        window.Show();

        if (show_minimized)
            window.Hide();
        else
            window.Activate();
    }

    private void CheckForSingleInstance()
    {
        var isNewInstance = false;
        singleInstanceMutex = new Mutex(true, @"Global\MacropadConfigurator", out isNewInstance);
        if (!isNewInstance)
        {
            MessageBox.Show("Second instance detected, shutting down");
            Current.Shutdown();
        }
    }

    public void HideWindow()
    {
        window.Hide();
    }
}
