using VisionFarm.Common.Extensions;

namespace VisionFarm.UI.Routes;

/// <summary>
/// 视图路由。
/// </summary>
public sealed class ViewRouter : IViewRouter
{
    private const string ViewNamespaceSuffix = "Pages";

    private readonly List<ViewRouteTable> _tables = [];

    public IReadOnlyList<ViewRouteTable> Routes => _tables;

    /// <summary>
    /// 构建路由。
    /// </summary>
    internal void Build()
    {
        var viewModels = Assembly.GetExecutingAssembly().GetAllTypesOf<ViewModelBase>();
        foreach (var vmType in viewModels)
        {
            var attr = vmType.GetCustomAttribute<ViewMetadataAttribute>(false);
            var viewType = attr?.ViewType ?? GetViewType(vmType);

            ViewRouteTable routeTable = new()
            {
                AliasName = attr?.AliasName ?? viewType?.Name,
                ViewModelType = vmType,
                ViewType = viewType,
            };
            _tables.Add(routeTable);
        }
    }

    private static Type? GetViewType(Type vmType)
    {
        // 对应规则
        // <assemblyName>.ViewModels.[My]ViewModel => <assemblyName>.Pages.[My]Page

        var assemblyName = vmType.Assembly.GetName().Name;
        var name = $"{assemblyName}.{ViewNamespaceSuffix}.{vmType.Name.Replace("ViewModel", "Page")}, {assemblyName}";
        return Type.GetType(name);
    }
}
