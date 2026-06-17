namespace VisionIndustry.UI.ViewModels;

/// <summary>
/// 菜单可选项。
/// </summary>
public sealed partial class MenuItemSelectedViewModel : ObservableObject
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
    /// 是否已选择
    /// </summary>
    [ObservableProperty]
    public partial bool IsSelected { get; set; }

    /// <summary>
    /// 可访问的标识集合。
    /// </summary>
    public IReadOnlyCollection<string> AccessCodes { get; init; } = [];
}
