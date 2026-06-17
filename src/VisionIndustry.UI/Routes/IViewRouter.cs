namespace VisionIndustry.UI.Routes;

/// <summary>
/// 视图路由。
/// </summary>
public interface IViewRouter
{
    /// <summary>
    /// 获取路由集合。
    /// </summary>
    public IReadOnlyList<ViewRouteTable> Routes { get; }
}
