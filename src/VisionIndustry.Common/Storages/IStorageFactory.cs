namespace VisionIndustry.Common.Storages;

/// <summary>
/// 数据本地存储工厂。
/// </summary>
public interface IStorageFactory
{
    /// <summary>
    /// 创建存储
    /// </summary>
    /// <remarks>
    /// 每次创建使用后一定要确保释放。
    /// <code>
    /// using var storage = StorageFactory.CreateStorage();
    /// </code>
    /// </remarks>
    /// <returns></returns>
    IStorage CreateStorage();
}
