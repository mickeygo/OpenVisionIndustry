namespace VisionFarm.UI.ViewModels;

/// <summary>
/// 登录页 ViewModel。
/// </summary>
public sealed partial class LoginViewModel : ViewModelBase
{
    /// <summary>
    /// 账号
    /// </summary>
    [ObservableProperty]
    public partial string? Username { get; set; }

    /// <summary>
    /// 密码
    /// </summary>
    [ObservableProperty]
    public partial string? Password { get; set; }
}
