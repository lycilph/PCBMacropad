using System.ComponentModel;

namespace MacropadConfigurator.ViewModels;

public interface IWindowLifecycleAware
{
    void OnLoaded();
    void OnClosing(CancelEventArgs e);
}
