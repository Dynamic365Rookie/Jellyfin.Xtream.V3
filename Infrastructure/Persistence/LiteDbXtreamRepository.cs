using LiteDB;

namespace Jellyfin.Xtream.Infrastructure.Persistence;

public sealed class LiteDbXtreamRepository<T> : IXtreamRepository<T>
    where T : class, IEntity
{
    private readonly ILiteCollection<T> _collection;
    private readonly LiteDatabase _db;

    public LiteDbXtreamRepository(LiteDatabase db, string name)
    {
        _db = db;
        _collection = db.GetCollection<T>(name);
        
        // Index pour améliorer les performances
        _collection.EnsureIndex(x => x.Id, unique: true);
        
        // Index pour les recherches par date de modification
        if (typeof(T).GetProperty("LastModified") != null)
        {
            _collection.EnsureIndex("LastModified");
        }
    }

    public bool HasChanged(T entity)
    {
        var existing = _collection.FindById(entity.Id);
        if (existing == null) return true;
        
        // Vérifier si LastModified existe et compare
        var lastModProp = typeof(T).GetProperty("LastModified");
        if (lastModProp != null)
        {
            var existingDate = (DateTime?)lastModProp.GetValue(existing);
            var newDate = (DateTime?)lastModProp.GetValue(entity);
            return existingDate != newDate;
        }
        
        return true;
    }

    public void Upsert(T entity)
    {
        _collection.Upsert(entity);
    }

    public void UpsertBatch(IEnumerable<T> entities)
    {
        const int batchSize = 1000;
        var batches = entities
            .Select((entity, index) => new { entity, index })
            .GroupBy(x => x.index / batchSize)
            .Select(g => g.Select(x => x.entity));

        foreach (var batch in batches)
        {
            _collection.Upsert(batch);
        }
    }

    public IEnumerable<T> GetAll()
    {
        return _collection.FindAll();
    }

    public T? GetById(int id)
    {
        return _collection.FindById(id);
    }

    public IEnumerable<T> GetByIds(IEnumerable<int> ids)
    {
        return _collection.Find(x => ids.Contains(x.Id));
    }

    public Dictionary<int, DateTime> GetLastModifiedMap()
    {
        var lastModProp = typeof(T).GetProperty("LastModified");
        if (lastModProp == null)
        {
            return new Dictionary<int, DateTime>();
        }

        var results = _collection.FindAll();
        return results.ToDictionary(
            x => x.Id,
            x => (DateTime)(lastModProp.GetValue(x) ?? DateTime.MinValue)
        );
    }

    public void DeleteNotInList(IEnumerable<int> validIds)
    {
        var validIdSet = new HashSet<int>(validIds);
        _collection.DeleteMany(x => !validIdSet.Contains(x.Id));
    }

    public int Count()
    {
        return _collection.Count();
    }
}

