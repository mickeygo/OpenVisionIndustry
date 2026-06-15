using Microsoft.Extensions.DependencyInjection;

namespace VisionFarm.Common.DI;

/// <summary>
/// 表示实现类会注册为服务，且具有 <see cref="ServiceLifetime.Scoped"/> 生命周期。
/// </summary>
public interface IScopedServiceLifetime
{
}
