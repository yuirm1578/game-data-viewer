using System.Globalization;
using System.Windows.Data;

namespace GameDataViewer.App.Converters;

/// <summary>bool → 한국어 텍스트. TrueText / FalseText 는 XAML에서 설정.</summary>
[ValueConversion(typeof(bool), typeof(string))]
public sealed class BoolToYesNoConverter : IValueConverter
{
    public string TrueText  { get; set; } = "예";
    public string FalseText { get; set; } = "아니오";

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        => value is true ? TrueText : FalseText;

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => Binding.DoNothing;
}
