using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace MacropadConfigurator.ViewModels;

public partial class NotifyIconViewModel(App app) : ObservableObject
{
    [RelayCommand]
    private void ShowWindow()
    {
        app.ShowWindow();
    }

    [RelayCommand]
    private void ExitApplication()
    {
        app.ExitApplication();
    }
}