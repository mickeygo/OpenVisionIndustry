using System.Linq.Expressions;

namespace VisionIndustry.Common.Storages;

/// <summary>
/// 本地存储查询。
/// </summary>
public interface IStorageQuery : IDisposable
{
    /// <summary>
    /// 通过 Id 获取单一实体。
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="id">实体Id</param>
    /// <returns></returns>
    T SingleById<T>(int id) where T : IStorageObject;

    /// <summary>
    /// 通过 Id 获取单一实体。
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="id">实体Id</param>
    /// <returns></returns>
    T? SingleOrDefaultById<T>(int id) where T : IStorageObject;

    /// <summary>
    /// 查询唯一的一个实体。
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="predicate">筛选条件</param>
    /// <returns></returns>
   T Single<T>(Expression<Func<T, bool>> predicate) where T : IStorageObject;

    /// <summary>
    /// 查询唯一的一个实体，没有找到则返回默认值。
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="predicate">筛选条件</param>
    /// <returns></returns>
    T? SingleOrDefault<T>(Expression<Func<T, bool>> predicate) where T : IStorageObject;

    /// <summary>
    /// 查询第一个实体。
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="predicate">筛选条件</param>
    /// <returns></returns>
    T First<T>(Expression<Func<T, bool>> predicate) where T : IStorageObject;

    /// <summary>
    /// 查询第一个实体，没有找到则返回默认值。
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="predicate">筛选条件</param>
    /// <returns></returns>
    T? FirstOrDefault<T>(Expression<Func<T, bool>> predicate) where T : IStorageObject;

    /// <summary>
    /// 查询实体集合。
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="predicate">筛选条件</param>
    /// <returns></returns>
    List<T> Fetch<T>(Expression<Func<T, bool>> predicate) where T : IStorageObject;
}
