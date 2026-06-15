using VisionFarm.Common.Storages;

namespace VisionFarm.Common.Slots.Profiles;

/// <summary>
/// 用户配置。
/// </summary>
public sealed class UserProfile : StorageObject
{
    /// <summary>
    /// 用户名
    /// </summary>
    public string? Username { get; set; }
}
