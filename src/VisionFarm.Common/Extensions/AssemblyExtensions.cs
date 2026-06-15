using System.Reflection;

namespace VisionFarm.Common.Extensions;

/// <summary>
/// 程序集扩展。
/// </summary>
public static class AssemblyExtensions
{
    extension(Assembly assembly)
    {
        /// <summary>
        /// 从指定程序集中检索所有的可构造的类型，这些类有实现或是继承指定的类型或接口，包括类自身。
        /// </summary>
        /// <typeparam name="TInterface">接口或基类，或是类本身。</typeparam>
        /// <returns></returns>
        public IReadOnlyCollection<Type> GetAllTypesOf<TInterface>()
        {
            var isAssignableToTInterface = typeof(TInterface).IsAssignableFrom;
            return [.. assembly
            .GetTypes()
            .Where(type => type.IsClass && !type.IsAbstract && !type.IsInterface && isAssignableToTInterface(type))];
        }

        /// <summary>
        /// 从指定程序集中检索所有的可构造的类型，这些类有设定指定的特性。
        /// </summary>
        /// <typeparam name="TAttribute"></typeparam>
        /// <param name="inherit">是否继承，默认 false。</param>
        /// <returns></returns>
        public IReadOnlyCollection<Type> GetAllAttributesOf<TAttribute>(bool inherit = false)
            where TAttribute : Attribute
        {
            return [.. assembly
            .GetTypes()
            .Where(type => type.IsClass && !type.IsAbstract && !type.IsInterface && Attribute.IsDefined(type, typeof(TAttribute), inherit))];
        }
    }
}
