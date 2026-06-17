using VisionIndustry.Common.Slots.Profiles;

namespace VisionIndustry.Common.Slots;

public static class IAppDataExtensions
{
    extension(IAppData appData)
    {
        /// <summary>
        /// 获取当前用户，未设置时为 null。
        /// </summary>
        /// <returns></returns>
        public string? GetUsername()
        {
            return appData.GetUser()?.Username;
        }

        /// <summary>
        /// 设置用户
        /// </summary>
        public bool SetUser(string uername)
        {
            return appData.SetUser(new UserProfile { Username = uername });
        }
    }
}
