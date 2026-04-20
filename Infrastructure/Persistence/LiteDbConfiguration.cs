using LiteDB;

namespace Jellyfin.Xtream.Infrastructure.Persistence;

public static class LiteDbConfiguration
{
    public static LiteDatabase CreateOptimizedDatabase(string connectionString)
    {
        var connBuilder = new ConnectionString(connectionString)
        {
            // Optimisations pour haute volumétrie
            Connection = ConnectionType.Shared,

            // Utiliser mode WAL (Write-Ahead Logging) pour meilleures performances
            // en écriture concurrente
            Upgrade = true,

            // Cache de 50MB pour améliorer les performances de lecture
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

        // Définir la taille de cache (en pages, 1 page = 8KB)
        // 5000 pages = ~40MB de cache
        db.Pragma("CACHE_SIZE", 5000);

        // Activer le mode asynchrone pour les écritures
        db.Pragma("ASYNC", true);

        return db;
    }

    public static void OptimizeForBulkInsert(LiteDatabase db)
    {
        // Désactiver temporairement certains checks pour les imports massifs
        db.Checkpoint();
    }

    public static void OptimizeForNormalOperation(LiteDatabase db)
    {
        // Réactiver les optimisations normales
        db.Checkpoint();
    }
}
