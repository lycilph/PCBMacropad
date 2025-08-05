using CommunityToolkit.Mvvm.ComponentModel;
using MacropadConfigurator.Models;
using MacropadConfigurator.Services;
using NLog;

namespace MacropadConfigurator.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private static readonly Logger logger = LogManager.GetCurrentClassLogger();

    public List<Layer> Layers;

    public MainViewModel(ApplicationService applicationService)
    {
        Layers = applicationService.Layers;
    }
}
