using VisionFarm.Common.Storages;

namespace VisionFarm.Common.Slots.Profiles;

/// <summary>
/// 菜单选项配置。
/// </summary>
public sealed class MenuProfile : StorageObject
{
    /// <summary>
    /// 菜单名称关键字集合
    /// </summary>
    public string[]? Keys { get; set; }
}
