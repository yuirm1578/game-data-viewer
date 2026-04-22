using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GameDataViewer.Core.Contracts;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;

namespace GameDataViewer.App.ViewModels;

/// <summary>차트 탭 VM — 등급 분포 파이 · 스탯 비교 바차트</summary>
public partial class ChartViewModel : ViewModelBase
{
    private readonly IChartDataService _chartService;

    [ObservableProperty] private PlotModel _itemGradeModel    = new();
    [ObservableProperty] private PlotModel _monsterGradeModel = new();
    [ObservableProperty] private PlotModel _topSkillsModel    = new();
    [ObservableProperty] private PlotModel _topMonstersModel  = new();

    public ChartViewModel(IChartDataService chartService)
    {
        _chartService = chartService;
    }

    [RelayCommand]
    private async Task LoadAsync()
    {
        SetBusy(true, "차트 데이터 로딩 중...");
        try
        {
            var t1 = _chartService.GetItemGradeDistributionAsync();
            var t2 = _chartService.GetMonsterGradeDistributionAsync();
            var t3 = _chartService.GetTopSkillsByMaxDamageAsync(10);
            var t4 = _chartService.GetTopMonstersByHpAsync(10);

            await Task.WhenAll(t1, t2, t3, t4);

            ItemGradeModel    = BuildGradePieModel("아이템 등급 분포",  t1.Result);
            MonsterGradeModel = BuildGradePieModel("몬스터 등급 분포", t2.Result);

            TopSkillsModel = BuildBarModel(
                "스킬 최대 데미지 TOP 10",
                t3.Result.Select(s => $"{s.Name} ({s.Class})").ToList(),
                t3.Result.Select(s => (double)s.MaxDamage).ToList(),
                OxyColor.FromArgb(210, 33, 150, 243));

            TopMonstersModel = BuildBarModel(
                "몬스터 HP TOP 10",
                t4.Result.Select(m => m.Name).ToList(),
                t4.Result.Select(m => (double)m.Hp).ToList(),
                OxyColor.FromArgb(210, 244, 67, 54));

            StatusMessage = "차트 로드 완료";
        }
        catch (Exception ex)
        {
            StatusMessage = $"차트 오류: {ex.Message}";
        }
        finally
        {
            SetBusy(false);
        }
    }

    // ─── 등급 → 차트 색상 (GradeToColorConverter 와 동일 팔레트) ─────────────
    private static OxyColor GetGradeColor(string grade) => grade switch
    {
        "전설" => OxyColor.FromArgb(230, 255, 152,   0),
        "영웅" => OxyColor.FromArgb(230,  33, 150, 243),
        "고급" => OxyColor.FromArgb(230,  76, 175,  80),
        "유물" => OxyColor.FromArgb(230, 156,  39, 176),
        "일반" => OxyColor.FromArgb(230, 158, 158, 158),
        _     => OxyColor.FromArgb(230, 158, 158, 158),
    };

    // ─── 파이차트 빌더 ────────────────────────────────────────────────────────
    private static PlotModel BuildGradePieModel(string title, IReadOnlyList<GradeCount> data)
    {
        var model = new PlotModel
        {
            Title              = title,
            Background         = OxyColors.Transparent,
            PlotAreaBackground = OxyColors.Transparent,
            TextColor          = OxyColors.WhiteSmoke,
            TitleColor         = OxyColors.WhiteSmoke,
        };

        var pieSeries = new PieSeries
        {
            StrokeThickness      = 1.5,
            InsideLabelPosition  = 0.72,
            InsideLabelFormat    = "{0}\n{1}개",
            OutsideLabelFormat   = null,
            TickHorizontalLength = 0,
            TickRadialLength     = 0,
        };

        foreach (var gc in data)
        {
            pieSeries.Slices.Add(new PieSlice(gc.Grade, gc.Count)
            {
                Fill       = GetGradeColor(gc.Grade),
                IsExploded = false,
            });
        }

        model.Series.Add(pieSeries);
        return model;
    }

    // ─── 수평 바차트 빌더 ────────────────────────────────────────────────────
    private static PlotModel BuildBarModel(
        string title,
        IReadOnlyList<string> labels,
        IReadOnlyList<double> values,
        OxyColor barColor)
    {
        var gridLine = OxyColor.FromArgb(45, 255, 255, 255);
        var dim      = OxyColor.FromArgb(90, 255, 255, 255);

        var model = new PlotModel
        {
            Title              = title,
            Background         = OxyColors.Transparent,
            PlotAreaBackground = OxyColors.Transparent,
            TextColor          = OxyColors.WhiteSmoke,
            TitleColor         = OxyColors.WhiteSmoke,
        };

        model.Axes.Add(new CategoryAxis
        {
            Position           = AxisPosition.Left,
            ItemsSource        = labels,
            TextColor          = OxyColors.WhiteSmoke,
            AxislineColor      = dim,
            TicklineColor      = dim,
            MajorGridlineColor = gridLine,
            MajorGridlineStyle = LineStyle.Solid,
        });

        model.Axes.Add(new LinearAxis
        {
            Position           = AxisPosition.Bottom,
            MinimumPadding     = 0,
            MaximumPadding     = 0.15,
            TextColor          = OxyColors.WhiteSmoke,
            AxislineColor      = dim,
            TicklineColor      = dim,
            MajorGridlineColor = gridLine,
            MajorGridlineStyle = LineStyle.Solid,
        });

        var series = new BarSeries
        {
            ItemsSource       = values.Select(v => new BarItem(v)).ToList(),
            FillColor         = barColor,
            LabelFormatString = "{0:#,0}",
        };

        model.Series.Add(series);
        return model;
    }
}
