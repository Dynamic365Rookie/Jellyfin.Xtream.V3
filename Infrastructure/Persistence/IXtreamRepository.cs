namespace Jellyfin.Xtream.Infrastructure.Persistence;

public interface IXtreamRepository<T> where T : IEntity
{
    bool HasChanged(T entity);
    void Upsert(T entity);
    
    // Batch operations pour améliorer les performances
    void UpsertBatch(IEnumerable<T> entities);
    IEnumerable<T> GetAll();
    T? GetById(int id);
    IEnumerable<T> GetByIds(IEnumerable<int> ids);
    Dictionary<int, DateTime> GetLastModifiedMap();
    void DeleteNotInList(IEnumerable<int> validIds);
    int Count();
}
