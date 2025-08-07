using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;

namespace MacropadConfigurator.Converters;

public class KeyConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is Key k)
            return k.ToString();
        else if (value is ModifierKeys mk)
            return mk.ToString();
        else
            return DependencyProperty.UnsetValue;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
