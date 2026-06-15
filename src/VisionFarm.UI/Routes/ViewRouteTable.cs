namespace VisionFarm.UI.Routes;

/// <summary>
/// 路由表。
/// </summary>
public sealed class ViewRouteTable
{
    /// <summary>
    /// 路由别名，若未设置则默认为 Pages 目录下按规则对应的 View 名称，若 View 未找到则为 null。
    /// </summary>
    public string? AliasName { get; init; }

    /// <summary>
    /// ViewModel 类型
    /// </summary>
    [NotNull]
    public Type? ViewModelType { get; init; }

    /// <summary>
    /// View 类型，若未设置则默认为 Pages 目录下按规则对应的 Page 类型，若 Page 未找到则为 null。
    /// </summary>
    public Type? ViewType { get; init; }
}
