namespace VisionFarm.Common.Storages;

/// <summary>
/// 数据本地存储工厂。
/// </summary>
internal sealed class DefaultStorageFactory : IStorageFactory
{
    public IStorage CreateStorage()
    {
        // 数据库文件名 mydata.db，使用 Shared 以方便在多线程中（或多进程）使用。
        return new StorageEngine("Filename=mydata.db;Connection=Shared");
    }
}
