using System.ComponentModel;
using System.Windows;
using MacropadConfigurator.ViewModels;
using Microsoft.Xaml.Behaviors;

namespace MacropadConfigurator.Behaviors;

public class WindowLifecycleBehavior : Behavior<Window>
{
    protected override void OnAttached()
    {
        base.OnAttached();
        AssociatedObject.DataContextChanged += AssociatedObjectDataContextChanged;
    }

    protected override void OnDetaching()
    {
        AssociatedObject.DataContextChanged -= AssociatedObjectDataContextChanged;
        base.OnDetaching();
    }

    private void AssociatedObjectDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (AssociatedObject.DataContext is IWindowLifecycleAware vm)
            AssociatedObject.Closing += AssociatedObjectClosing;
        else
            AssociatedObject.Closing -= AssociatedObjectClosing;
    }

    private void AssociatedObjectClosing(object? sender, CancelEventArgs e)
    {
        if (AssociatedObject.DataContext is IWindowLifecycleAware vm)
        {
            vm.OnClosing(e);
        }
    }
}
