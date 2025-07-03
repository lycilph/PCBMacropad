using System.ComponentModel;
using System.Windows;
using MacropadConfigurator.ViewModels;
using Microsoft.Xaml.Behaviors;

namespace MacropadConfigurator.Behaviors;

public class LifecycleBehavior : Behavior<Window>
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
        if (AssociatedObject.DataContext is ILifecycleAware vm)
            AssociatedObject.Closing += AssociatedObjectClosing;
        else
            AssociatedObject.Closing -= AssociatedObjectClosing;
    }

    private void AssociatedObjectClosing(object? sender, CancelEventArgs e)
    {
        if (AssociatedObject.DataContext is ILifecycleAware vm)
        {
            vm.OnClosing(e);
        }
    }
}
