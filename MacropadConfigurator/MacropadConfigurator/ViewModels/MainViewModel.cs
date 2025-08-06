using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using MacropadConfigurator.Messages;
using MacropadConfigurator.Models;
using MacropadConfigurator.Services;
using NLog;

namespace MacropadConfigurator.ViewModels;

public partial class MainViewModel : ObservableRecipient, IRecipient<LogMessage>
{
    private static readonly Logger logger = LogManager.GetCurrentClassLogger();

    public List<Layer> Layers { get; set; }

    [ObservableProperty]
    private Layer selectedLayer;

    [ObservableProperty]
    private ObservableCollection<string> log = [];

    public MainViewModel(ApplicationService applicationService)
    {
        Layers = applicationService.Layers;
        selectedLayer = Layers.First();

        IsActive = true;
    }

    public void Receive(LogMessage message)
    {
        logger.Info($"Got message: {message.Text}");

        Log.Add(message.Text);
    }
}
