namespace VisionFarm.UI.ViewModels;

/// <summary>
/// 状态栏 ViewModel。
/// </summary>
public sealed partial class StatusBarViewModel : ViewModelBase
{
    /// <summary>
    /// 应用程序版本号
    /// </summary>
    public string Version => typeof(ViewModelBase).Assembly.GetName().Version?.ToString(4) ?? "0.0.0.0";

    /// <summary>
    /// 服务器地址
    /// </summary>
    [ObservableProperty]
    public partial string? Server { get; set; } = "127.0.0.1";

    /// <summary>
    /// 服务器连接状态 Off/Blinking/Good/Bad
    /// </summary>
    [ObservableProperty]
    public partial LedState ServerConnectionState { get; set; } = LedState.Blinking;

    public override void Dispose()
    {
        WeakReferenceMessenger.Default.UnregisterAll(this);
    }
}
