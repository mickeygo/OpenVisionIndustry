using System.Linq.Expressions;
using LiteDB;

namespace VisionIndustry.Common.Storages;

/// <summary>
/// 数据存储引擎。
/// </summary>
internal sealed class StorageEngine(string connectionString) : IStorage
{
    public ILiteDatabase Database { get; } = new LiteDatabase(connectionString);

    public ILiteQueryable<T> Query<T>() where T : IStorageObject
    {
        return Database.GetCollection<T>().Query();
    }

    public T SingleById<T>(int id) where T : IStorageObject
    {
        return Database.GetCollection<T>().Query()
            .Where("_id = @0", id)
            .Single();
    }

    public T? SingleOrDefaultById<T>(int id) where T : IStorageObject
    {
        return Database.GetCollection<T>().Query()
            .Where("_id = @0", id)
            .SingleOrDefault();
    }

    public T Single<T>(Expression<Func<T, bool>> predicate) where T : IStorageObject
    {
        return Query<T>().Where(predicate).Single();
    }

    public T? SingleOrDefault<T>(Expression<Func<T, bool>> predicate) where T : IStorageObject
    {
        return Query<T>().Where(predicate).SingleOrDefault();
    }

    public T First<T>(Expression<Func<T, bool>> predicate) where T : IStorageObject
    {
        return Query<T>().Where(predicate).First();
    }

    public T? FirstOrDefault<T>(Expression<Func<T, bool>> predicate) where T : IStorageObject
    {
        return Query<T>().Where(predicate).FirstOrDefault();
    }

    public List<T> Fetch<T>(Expression<Func<T, bool>> predicate) where T : IStorageObject
    {
        return Query<T>().Where(predicate).ToList();
    }

    public int Insert<T>(T entity) where T : IStorageObject
    {
        return Database.GetCollection<T>().Insert(entity);
    }

    public int Insert<T>(IEnumerable<T> entities) where T : IStorageObject
    {
        return Database.GetCollection<T>().Insert(entities);
    }

    public bool Update<T>(T entity) where T : IStorageObject
    {
        return Database.GetCollection<T>().Update(entity);
    }

    public int Update<T>(IEnumerable<T> entities) where T : IStorageObject
    {
        return Database.GetCollection<T>().Update(entities);
    }

    public bool InsertOrUpdate<T>(T entity) where T : IStorageObject
    {
        return Database.GetCollection<T>().Upsert(entity);
    }

    public int InsertOrUpdate<T>(IEnumerable<T> entities) where T : IStorageObject
    {
        return Database.GetCollection<T>().Upsert(entities);
    }

    public bool Delete<T>(int id) where T : IStorageObject
    {
        return Database.GetCollection<T>().Delete(id);
    }

    public int DeleteMany<T>(Expression<Func<T, bool>> predicate) where T : IStorageObject
    {
        return Database.GetCollection<T>().DeleteMany(predicate);
    }

    public bool EnsureIndex<T, K>(Expression<Func<T, K>> keySelector, bool unique = false) where T : IStorageObject
    {
        return Database.GetCollection<T>().EnsureIndex(keySelector, unique);
    }

    public void Dispose()
    {
        Database.Dispose();
        GC.SuppressFinalize(this);
    }
}
