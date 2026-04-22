using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace GameDataViewer.App.Converters;

/// <summary>등급 문자열 → 해당 등급을 나타내는 SolidColorBrush 반환</summary>
[ValueConversion(typeof(string), typeof(SolidColorBrush))]
public sealed class GradeToColorConverter : IValueConverter
{
    private static readonly SolidColorBrush NormalBrush   = Freeze(0x9E, 0x9E, 0x9E); // 일반 — 회색
    private static readonly SolidColorBrush AdvancedBrush = Freeze(0x4C, 0xAF, 0x50); // 고급 — 초록
    private static readonly SolidColorBrush HeroBrush     = Freeze(0x21, 0x96, 0xF3); // 영웅 — 파란
    private static readonly SolidColorBrush LegendBrush   = Freeze(0xFF, 0x98, 0x00); // 전설 — 주황
    private static readonly SolidColorBrush AncientBrush  = Freeze(0x9C, 0x27, 0xB0); // 유물 — 보라
    private static readonly SolidColorBrush DefaultBrush  = Freeze(0xBD, 0xBD, 0xBD);

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        => (value as string) switch
        {
            "일반" => NormalBrush,
            "고급" => AdvancedBrush,
            "영웅" => HeroBrush,
            "전설" => LegendBrush,
            "유물" => AncientBrush,
            _      => DefaultBrush
        };

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => Binding.DoNothing;

    private static SolidColorBrush Freeze(byte r, byte g, byte b)
    {
        var brush = new SolidColorBrush(Color.FromRgb(r, g, b));
        brush.Freeze();
        return brush;
    }
}
