namespace VisionIndustry.UI;

/// <summary>
/// 应用菜单。
/// </summary>
public sealed class AppMenus
{
    /// <summary>
    /// 应用菜单
    /// </summary>
    public static readonly IReadOnlyCollection<AppMenuItem> Menus = [
        new AppMenuItem
        {
            MenuHeader = "工作台",
            Key = "WorkbenchPage",
            IconName = "SemiIconCommand",
            AccessCodes = ["WORKBENCH"],
        },
        new AppMenuItem
        {
            MenuHeader = "图标",
            Key = "IconPage",
            IconName = "SemiIconEmoji",
            AccessCodes = [],
        },
    ];
}

/// <summary>
/// 菜单项
/// </summary>
public sealed class AppMenuItem
{
    /// <summary>
    /// 菜单名称
    /// </summary>
    [NotNull]
    public string? MenuHeader { get; init; }

    /// <summary>
    /// 菜单图标
    /// </summary>
    [NotNull]
    public string? IconName { get; init; }

    /// <summary>
    /// 菜单名称关键字
    /// </summary>
    [NotNull]
    public string? Key { get; init; }

    /// <summary>
    /// Badge 名称
    /// </summary>
    public string? Badge { get; init; }

    /// <summary>
    /// 分割线
    /// </summary>
    public bool IsSeparator { get; init; }

    /// <summary>
    /// 可访问的标识集合。
    /// </summary>
    public IReadOnlyCollection<string> AccessCodes { get; init; } = [];

    /// <summary>
    /// 子菜单
    /// </summary>
    public IReadOnlyCollection<AppMenuItem> Children { get; init; } = [];
}
