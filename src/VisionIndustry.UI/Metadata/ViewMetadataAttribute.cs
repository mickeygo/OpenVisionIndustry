using Avalonia.Controls;

namespace VisionIndustry.UI.Metadata;

/// <summary>
/// ViewModel 对应 View 的元数据。
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class ViewMetadataAttribute<T> : ViewMetadataAttribute
    where T : ContentControl
{
    public ViewMetadataAttribute() : base(typeof(T))
    {
    }
}

/// <summary>
/// ViewModel 对应 Page 的元数据。
/// </summary>
/// <param name="viewType">视图类型。</param>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public class ViewMetadataAttribute(Type viewType) : Attribute
{
    /// <summary>
    /// 页面类型。
    /// </summary>
    public Type ViewType => viewType;

    /// <summary>
    /// View 别名。
    /// </summary>
    public string? AliasName { get; init; }

    /// <summary>
    /// 视图生命周期，默认为 <see cref="ServiceLifetime.Transient"/>。
    /// </summary>
    public ServiceLifetime Lifetime { get; init; } = ServiceLifetime.Transient;
}
