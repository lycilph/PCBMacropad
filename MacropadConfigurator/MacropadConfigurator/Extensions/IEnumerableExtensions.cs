using System.Collections.ObjectModel;

namespace MacropadConfigurator.Extensions;

public static class IEnumerableExtensions
{
    public static ObservableCollection<T> ToObservableCollection<T>(this IEnumerable<T> list)
    {
        return [.. list];
    }
}