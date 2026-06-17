using VisionIndustry.Common.Slots.Profiles;
using VisionIndustry.Common.Storages;

namespace VisionIndustry.Common.Slots;

/// <summary>
/// 应用程序本地数据 
/// </summary>
internal sealed class AppData(IStorageFactory storageFactory) : IAppData
{
    public MenuProfile? GetMenus()
    {
        using var storage = storageFactory.CreateStorage();
        return storage.SingleOrDefault<MenuProfile>(m => m.Id == 1);
    }

    public UserProfile? GetUser()
    {
        using var storage = storageFactory.CreateStorage();
        return storage.SingleOrDefault<UserProfile>(m => m.Id == 1);
    }

    public bool SetMenus(MenuProfile data)
    {
        using var storage = storageFactory.CreateStorage();
        data.Id = 1;
        return storage.InsertOrUpdate(data);
    }

    public bool SetUser(UserProfile data)
    {
        using var storage = storageFactory.CreateStorage();
        data.Id = 1;
        return storage.InsertOrUpdate(data);
    }
}
