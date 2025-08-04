using CommunityToolkit.Mvvm.ComponentModel;
using NLog;

namespace MacropadConfigurator.ViewModels;

public partial class ShellViewModel : ObservableObject
{
    private static readonly Logger logger = LogManager.GetCurrentClassLogger();

    private readonly MainViewModel mainViewModel;
    
    [ObservableProperty]
    private ObservableObject content;

    public ShellViewModel(MainViewModel mainViewModel)
    {
        this.mainViewModel = mainViewModel;

        Content = mainViewModel;
    }
}
