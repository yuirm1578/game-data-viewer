using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace GameDataViewer.App.Converters;

/// <summary>bool → Visibility 변환기. Invert=true 이면 false일 때 Visible을 반환한다.</summary>
[ValueConversion(typeof(bool), typeof(Visibility))]
public class BoolToVisibilityConverter : IValueConverter
{
    /// <summary>true이면 Visible, false이면 Collapsed (Invert=true 시 반전)</summary>
    public bool Invert { get; set; }

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        bool b = value is bool bv && bv;
        if (Invert) b = !b;
        return b ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => DependencyProperty.UnsetValue;
}
