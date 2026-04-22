using System.Windows.Threading;
using CommunityToolkit.Mvvm.ComponentModel;

namespace GameDataViewer.App.ViewModels;

/// <summary>모든 ViewModel의 공통 베이스</summary>
public abstract partial class ViewModelBase : ObservableObject
{
    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    [ObservableProperty]
    private bool _isError;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    // IsBusy 변경 시 파생 클래스의 IsEmpty 바인딩도 갱신
    partial void OnIsBusyChanged(bool value)
        => OnPropertyChanged("IsEmpty");

    // ── 디바운스 헬퍼 ──────────────────────────────────────────
    private DispatcherTimer? _debounceTimer;

    /// <summary>
    /// action 을 delay 이후 한 번만 실행한다 (검색 입력 디바운스용).
    /// 연속 호출 시 타이머가 리셋되어 마지막 호출 기준으로 실행된다.
    /// </summary>
    protected void Debounce(Action action, TimeSpan? delay = null)
    {
        _debounceTimer?.Stop();
        _debounceTimer = new DispatcherTimer
        {
            Interval = delay ?? TimeSpan.FromMilliseconds(300)
        };
        _debounceTimer.Tick += (_, _) =>
        {
            _debounceTimer.Stop();
            action();
        };
        _debounceTimer.Start();
    }

    protected void SetBusy(bool busy, string message = "")
    {
        IsBusy = busy;
        StatusMessage = message;
    }

    protected void SetError(string message)
    {
        IsError = true;
        ErrorMessage = message;
        StatusMessage = message;
    }

    protected void ClearError()
    {
        IsError = false;
        ErrorMessage = string.Empty;
    }
}