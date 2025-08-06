using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace MacropadConfigurator.Behaviors;

public static class ScrollViewerBehavior
{
    public static readonly DependencyProperty AutoScrollProperty =
        DependencyProperty.RegisterAttached(
            "AutoScroll",
            typeof(bool),
            typeof(ScrollViewerBehavior),
            new PropertyMetadata(false, OnAutoScrollChanged));

    public static bool GetAutoScroll(DependencyObject obj) =>
        (bool)obj.GetValue(AutoScrollProperty);

    public static void SetAutoScroll(DependencyObject obj, bool value) =>
        obj.SetValue(AutoScrollProperty, value);

    private static void OnAutoScrollChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is ItemsControl itemsControl && e.NewValue is true)
        {
            itemsControl.Loaded += (_, __) =>
            {
                var scrollViewer = FindScrollViewer(itemsControl);
                if (scrollViewer != null && itemsControl.ItemsSource is INotifyCollectionChanged obs)
                {
                    obs.CollectionChanged += (_, args) =>
                    {
                        scrollViewer.ScrollToEnd();
                    };
                }
            };
        }
    }

    private static ScrollViewer? FindScrollViewer(DependencyObject d)
    {
        DependencyObject? current = d;
        while (current != null)
        {
            if (current is ScrollViewer sv)
                return sv;

            current = VisualTreeHelper.GetParent(current);
        }

        return null;
    }
}
