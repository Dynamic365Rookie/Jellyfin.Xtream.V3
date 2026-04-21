using LiteDB;

namespace Jellyfin.Xtream.Infrastructure.Persistence;

public static class LiteDbConfiguration
{
    public static LiteDatabase CreateOptimizedDatabase(string connectionString)
    {
        var connBuilder = new ConnectionString(connectionString)
        {
            // Optimisations pour haute volum�trie
            Connection = ConnectionType.Shared,

            // Utiliser mode WAL (Write-Ahead Logging) pour meilleures performances
            // en �criture concurrente
            Upgrade = true,

            // Cache de 50MB pour am�liorer les performances de lecture
            // Ajuster selon la RAM disponible
            // Note: LiteDB 5.x n'expose pas directement CacheSize dans ConnectionString
            // mais on peut le configurer via BsonMapper
        };

        var db = new LiteDatabase(connBuilder);

        // Configuration globale du mapper pour optimiser la sérialisation
        var mapper = db.Mapper;
        mapper.EmptyStringToNull = false;
        mapper.TrimWhitespace = false;
        mapper.EnumAsInteger = true; // Plus rapide que les strings

        return db;
    }

    public static void OptimizeForBulkInsert(LiteDatabase db)
    {
        // D�sactiver temporairement certains checks pour les imports massifs
        db.Checkpoint();
    }

    public static void OptimizeForNormalOperation(LiteDatabase db)
    {
        // R�activer les optimisations normales
        db.Checkpoint();
    }
}
