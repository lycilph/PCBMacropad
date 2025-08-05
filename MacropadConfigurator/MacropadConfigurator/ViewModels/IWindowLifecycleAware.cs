using System.ComponentModel;

namespace MacropadConfigurator.ViewModels;

public interface IWindowLifecycleAware
{
    void OnClosing(CancelEventArgs e);
}
