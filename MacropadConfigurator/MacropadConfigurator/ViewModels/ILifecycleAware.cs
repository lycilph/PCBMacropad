using System.ComponentModel;

namespace MacropadConfigurator.ViewModels;

public interface ILifecycleAware
{
    void OnClosing(CancelEventArgs e);
}
