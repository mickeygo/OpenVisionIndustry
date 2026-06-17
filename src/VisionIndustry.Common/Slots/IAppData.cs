using VisionIndustry.Common.Slots.Profiles;

namespace VisionIndustry.Common.Slots;

/// <summary>
/// 应用程序本地数据
/// </summary>
public interface IAppData
{
    /// <summary>
    /// 获取当前配置菜单。
    /// </summary>
    /// <returns></returns>
    MenuProfile? GetMenus();

    /// <summary>
    /// 获取当前用户。
    /// </summary>
    /// <returns></returns>
    UserProfile? GetUser();

    /// <summary>
    /// 设置菜单。
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    bool SetMenus(MenuProfile data);

    /// <summary>
    /// 设置当前用户。
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    bool SetUser(UserProfile data);
}
