using MacropadConfigurator.ViewModels;
using Microsoft.Xaml.Behaviors;
using System.Windows;
using System.Windows.Controls;

namespace MacropadConfigurator.Behaviors;

public class LoadedBehavior : Behavior<UserControl>
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
        if (AssociatedObject.DataContext is ILoadedAware vm)
            AssociatedObject.Loaded += AssociatedObjectLoading;
        else
            AssociatedObject.Loaded -= AssociatedObjectLoading;
    }

    private void AssociatedObjectLoading(object sender, RoutedEventArgs e)
    {
        if (AssociatedObject.DataContext is ILoadedAware vm)
        {
            vm.OnLoaded();
        }
    }

}
