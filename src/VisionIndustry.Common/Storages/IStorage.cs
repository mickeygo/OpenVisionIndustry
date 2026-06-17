using System.Linq.Expressions;

namespace VisionIndustry.Common.Storages;

/// <summary>
/// 数据本地存储。
/// </summary>
public interface IStorage : IStorageQuery, IDisposable
{
    /// <summary>
    /// 新增实体。
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="entity">要新增的实体</param>
    /// <returns>新增后的实体 Id</returns>
    int Insert<T>(T entity) where T : IStorageObject;

    /// <summary>
    /// 新增多个实体。
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="entities">要新增的实体集合</param>
    /// <returns></returns>
    int Insert<T>(IEnumerable<T> entities) where T : IStorageObject;

    /// <summary>
    /// 更新实体。
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="entity">要更新的实体</param>
    /// <returns></returns>
    bool Update<T>(T entity) where T : IStorageObject;

    /// <summary>
    /// 更新多个实体。
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="entities">要更新的实体集合</param>
    /// <returns></returns>
    int Update<T>(IEnumerable<T> entities) where T : IStorageObject;

    /// <summary>
    /// 新增实体，若存在则更新。
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="entity"></param>
    /// <returns></returns>
    bool InsertOrUpdate<T>(T entity) where T : IStorageObject;

    /// <summary>
    /// 新增多个实体，若存在则更新。
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="entities"></param>
    /// <returns></returns>
    int InsertOrUpdate<T>(IEnumerable<T> entities) where T : IStorageObject;

    /// <summary>
    /// 删除实体。
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="id">要删除的实体 Id</param>
    /// <returns></returns>
    bool Delete<T>(int id) where T : IStorageObject;

    /// <summary>
    /// 删除多个实体。
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="predicate">删除条件</param>
    /// <returns></returns>
    int DeleteMany<T>(Expression<Func<T, bool>> predicate) where T : IStorageObject;

    /// <summary>
    /// 检查指定字段是否已创建索引，若不存在则创建。
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="K"></typeparam>
    /// <param name="keySelector">要创建索引的字段</param>
    /// <param name="unique">是否是唯一索引</param>
    /// <returns></returns>
    bool EnsureIndex<T, K>(Expression<Func<T, K>> keySelector, bool unique = false) where T : IStorageObject;
}
