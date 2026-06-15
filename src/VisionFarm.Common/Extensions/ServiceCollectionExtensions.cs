using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using VisionFarm.Common.DI;
using VisionFarm.Common.Slots;
using VisionFarm.Common.Storages;

namespace VisionFarm.Common.Extensions;

public static class ServiceCollectionExtensions
{
    extension(IHostBuilder builder)
    {
        /// <summary>
        /// 注入 Core 服务。
        /// </summary>
        /// <returns></returns>
        public IHostBuilder UseCommonSetup()
        {
            builder.ConfigureServices((hostBuilder, services) =>
            {
                var assembly = typeof(ServiceCollectionExtensions).Assembly;

                services.RegisterForInterfaceAndType<ITransientServiceLifetime>(assembly, ServiceLifetime.Transient);
                services.RegisterForInterfaceAndType<IScopedServiceLifetime>(assembly, ServiceLifetime.Scoped);
                services.RegisterForInterfaceAndType<ISingletonServiceLifetime>(assembly, ServiceLifetime.Singleton);

                // 注册全局数据
                services.AddSingleton<IAppData, AppData>();

                // 注册全局数据
                services.AddSingleton<IStorageFactory, DefaultStorageFactory>();
            });

            return builder;
        }
    }

    extension(IServiceCollection services)
    {
        /// <summary>
        /// 注册接口与对应服务，其中该服务有实现对应的 <typeparamref name="TInterface"/> 类或接口。
        /// 类型没有接口或仅有唯一且不同于或派生于 <typeparamref name="TInterface"/> 的接口，
        /// 若没有接口时则仅注册类型自身，存在接口时则会注册接口与对应类型。
        /// </summary>
        /// <typeparam name="TInterface"></typeparam>
        /// <param name="assembly">检索的程序集。</param>
        /// <param name="lifetime">服务的生命周期。</param>
        /// <returns></returns>
        public IServiceCollection RegisterForInterfaceAndType<TInterface>(Assembly assembly, ServiceLifetime lifetime = ServiceLifetime.Transient)
        {
            var types = assembly.GetAllTypesOf<TInterface>();
            foreach (var type in types)
            {
                var @interface = type.GetInterfaces().SingleOrDefault(t => t != typeof(TInterface) && t != typeof(IDisposable) && t != typeof(IAsyncDisposable));
                services.Add(new ServiceDescriptor(@interface ?? type, type, lifetime));
            }

            return services;
        }

        /// <summary>
        /// 注册服务，该服务有实现对应的 <typeparamref name="TInterface"/> 对象，仅注册类型自身。
        /// </summary>
        /// <typeparam name="TInterface">要实现的类或接口。</typeparam>
        /// <param name="assembly">检索的程序集。</param>
        /// <param name="lifetime">服务的生命周期。</param>
        /// <returns></returns>
        public IServiceCollection RegisterForType<TInterface>(Assembly assembly, ServiceLifetime lifetime = ServiceLifetime.Transient)
        {
            var types = assembly.GetAllTypesOf<TInterface>();
            foreach (var type in types)
            {
                services.Add(new ServiceDescriptor(type, type, lifetime));
            }

            return services;
        }
    }
}
