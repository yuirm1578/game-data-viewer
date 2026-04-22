using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace GameDataViewer.App.Converters;

/// <summary>null → Collapsed, 값 있음 → Visible. Invert=true 이면 반전.</summary>
[ValueConversion(typeof(object), typeof(Visibility))]
public sealed class NullToVisibilityConverter : IValueConverter
{
    public bool Invert { get; set; }

    public object Convert(object? value, Type targetType, object parameter, CultureInfo culture)
    {
        bool hasValue = value is not null;
        return (Invert ? !hasValue : hasValue) ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => Binding.DoNothing;
}
