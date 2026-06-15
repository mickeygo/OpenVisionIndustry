using VisionFarm.Common.Extensions;
using VisionFarm.UI.Routes;

namespace VisionFarm.UI.Extensions;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// 注入 UI 应用服务。 
    /// </summary>
    /// <param name="builder"></param>
    /// <returns></returns>
    public static IHostBuilder UseUISetup(this IHostBuilder builder)
    {
        builder.ConfigureServices((_, services) =>
        {
            services.AddViewModelServices();
            services.AddViewRouter();

        });

        return builder;
    }

    /// <summary>
    /// 注入 ViewModel 服务集合。
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    private static IServiceCollection AddViewModelServices(this IServiceCollection services)
    {
        var descriptors = Assembly.GetExecutingAssembly().GetAllTypesOf<IViewModelObject>().Select(t =>
        {
            var lifetime = t.IsDefined(typeof(VmKeepAliveAttribute), false) ? ServiceLifetime.Singleton : ServiceLifetime.Transient;
            return new ServiceDescriptor(t, t, lifetime);
        });

        foreach (var descriptor in descriptors)
        {
            services.Add(descriptor);
        }

        return services;
    }

    /// <summary>
    /// 添加视图路由器。
    /// </summary>
    /// <param name="services"></param>
    private static IServiceCollection AddViewRouter(this IServiceCollection services)
    {
        services.AddSingleton<IViewRouter>(_ =>
        {
            ViewRouter router = new();
            router.Build();

            return router;
        });

        return services;
    }
}
