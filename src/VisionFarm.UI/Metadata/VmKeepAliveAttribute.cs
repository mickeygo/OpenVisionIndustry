namespace VisionFarm.UI.Metadata;

/// <summary>
/// 设置 ViewModel 为保活状态，切换菜单后依然保持数据，生命周期为 <see cref="ServiceLifetime.Singleton"/> 。
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class VmKeepAliveAttribute : Attribute
{
}
