using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace GameDataViewer.App.Converters;

/// <summary>속성(Element) 문자열 → 해당 속성 색상 SolidColorBrush 반환</summary>
[ValueConversion(typeof(string), typeof(SolidColorBrush))]
public sealed class ElementToColorConverter : IValueConverter
{
    private static readonly SolidColorBrush FireBrush  = Freeze(0xFF, 0x57, 0x22); // 화 — 주황
    private static readonly SolidColorBrush WaterBrush = Freeze(0x42, 0xA5, 0xF5); // 수 — 파랑
    private static readonly SolidColorBrush WindBrush  = Freeze(0x66, 0xBB, 0x6A); // 풍 — 초록
    private static readonly SolidColorBrush EarthBrush = Freeze(0xA1, 0x88, 0x7F); // 지 — 갈색
    private static readonly SolidColorBrush LightBrush = Freeze(0xFF, 0xD7, 0x40); // 빛 — 금색
    private static readonly SolidColorBrush DarkBrush  = Freeze(0xCE, 0x93, 0xD8); // 암 — 보라
    private static readonly SolidColorBrush NoneBrush  = Freeze(0x9E, 0x9E, 0x9E); // 무 — 회색

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        => (value as string) switch
        {
            "화" => FireBrush,
            "수" => WaterBrush,
            "풍" => WindBrush,
            "지" => EarthBrush,
            "빛" => LightBrush,
            "암" => DarkBrush,
            _    => NoneBrush
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
