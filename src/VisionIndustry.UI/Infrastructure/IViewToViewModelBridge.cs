using Avalonia;

namespace VisionIndustry.UI.Infrastructure;

/// <summary>
/// 将 View 注入 ViewModel 的桥接器，以便能获取对应 View 的 TopLevel。
/// </summary>
public interface IViewToViewModelBridge
{
    /// <summary>
    /// 获取或设置对应 View。
    /// </summary>
    Visual? View { get; set; }
}
