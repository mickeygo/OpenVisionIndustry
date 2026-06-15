namespace VisionFarm.Common.Storages;

/// <summary>
/// 表示实现类为可存储的对象。
/// </summary>
public interface IStorageObject
{
    /// <summary>
    /// Id 主键。
    /// </summary>
    public int Id { get; set; }
}
