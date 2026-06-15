namespace VisionFarm.Common.Storages;

/// <summary>
/// 可存储的实体基类。
/// </summary>
public abstract class StorageObject : IStorageObject
{
    public int Id { get; set; }
}
