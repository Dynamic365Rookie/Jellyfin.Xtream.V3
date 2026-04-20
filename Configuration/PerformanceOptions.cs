namespace Jellyfin.Xtream.Configuration;

/// <summary>
/// Configuration de performance pour le plugin Xtream
/// </summary>
public sealed class PerformanceOptions
{
    /// <summary>
    /// Taille des lots pour les opérations d'insertion en base de données
    /// Valeur recommandée: 1000 pour un bon équilibre performance/mémoire
    /// Augmenter pour plus de vitesse (mais plus de mémoire)
    /// Diminuer si problèmes de mémoire
    /// </summary>
    public int BatchSize { get; set; } = 1000;

    /// <summary>
    /// Nombre maximum d'entrées dans le cache mémoire
    /// </summary>
    public int MaxCacheEntries { get; set; } = 10000;

    /// <summary>
    /// Durée d'expiration par défaut du cache en heures
    /// </summary>
    public int CacheExpirationHours { get; set; } = 2;

    /// <summary>
    /// Limite de mémoire en MB avant déclenchement du garbage collection
    /// </summary>
    public int MaxMemoryMB { get; set; } = 2048;

    /// <summary>
    /// Seuil d'utilisation mémoire (en pourcentage) avant alerte
    /// Valeur entre 0 et 1 (0.8 = 80%)
    /// </summary>
    public double MemoryThreshold { get; set; } = 0.8;

    /// <summary>
    /// Taille du cache LiteDB en pages (1 page = 8KB)
    /// 5000 pages = ~40MB
    /// </summary>
    public int LiteDbCacheSize { get; set; } = 5000;

    /// <summary>
    /// Degré maximum de parallélisme pour les opérations simultanées
    /// Recommandé: 4-8 selon le CPU
    /// </summary>
    public int MaxDegreeOfParallelism { get; set; } = 4;

    /// <summary>
    /// Activer le mode retry automatique sur les appels API
    /// </summary>
    public bool EnableApiRetry { get; set; } = true;

    /// <summary>
    /// Nombre maximum de tentatives pour les appels API
    /// </summary>
    public int MaxApiRetries { get; set; } = 3;

    /// <summary>
    /// Timeout pour les appels API en secondes
    /// </summary>
    public int ApiTimeoutSeconds { get; set; } = 300; // 5 minutes

    /// <summary>
    /// Taille du buffer JSON pour la désérialisation (en bytes)
    /// </summary>
    public int JsonBufferSize { get; set; } = 65536; // 64KB

    /// <summary>
    /// Fréquence de nettoyage du cache en minutes
    /// </summary>
    public int CacheCleanupFrequencyMinutes { get; set; } = 15;

    /// <summary>
    /// Activer le logging détaillé des performances
    /// </summary>
    public bool EnablePerformanceLogging { get; set; } = true;

    /// <summary>
    /// Activer le monitoring de la mémoire
    /// </summary>
    public bool EnableMemoryMonitoring { get; set; } = true;

    /// <summary>
    /// Supprimer automatiquement les entités obsolètes
    /// (non présentes dans la dernière synchronisation)
    /// </summary>
    public bool AutoDeleteObsoleteEntities { get; set; } = true;

    /// <summary>
    /// Valider la configuration
    /// </summary>
    public void Validate()
    {
        if (BatchSize < 100 || BatchSize > 10000)
            throw new ArgumentException("BatchSize doit être entre 100 et 10000", nameof(BatchSize));

        if (MaxCacheEntries < 1000 || MaxCacheEntries > 100000)
            throw new ArgumentException("MaxCacheEntries doit être entre 1000 et 100000", nameof(MaxCacheEntries));

        if (CacheExpirationHours < 1 || CacheExpirationHours > 24)
            throw new ArgumentException("CacheExpirationHours doit être entre 1 et 24", nameof(CacheExpirationHours));

        if (MaxMemoryMB < 512 || MaxMemoryMB > 8192)
            throw new ArgumentException("MaxMemoryMB doit être entre 512 et 8192", nameof(MaxMemoryMB));

        if (MemoryThreshold < 0.5 || MemoryThreshold > 0.95)
            throw new ArgumentException("MemoryThreshold doit être entre 0.5 et 0.95", nameof(MemoryThreshold));

        if (MaxDegreeOfParallelism < 1 || MaxDegreeOfParallelism > 16)
            throw new ArgumentException("MaxDegreeOfParallelism doit être entre 1 et 16", nameof(MaxDegreeOfParallelism));

        if (MaxApiRetries < 1 || MaxApiRetries > 10)
            throw new ArgumentException("MaxApiRetries doit être entre 1 et 10", nameof(MaxApiRetries));

        if (ApiTimeoutSeconds < 30 || ApiTimeoutSeconds > 600)
            throw new ArgumentException("ApiTimeoutSeconds doit être entre 30 et 600", nameof(ApiTimeoutSeconds));
    }

    /// <summary>
    /// Configuration par défaut optimisée pour la volumétrie cible
    /// (15K films, 8.5K séries, 1.5K chaînes)
    /// </summary>
    public static PerformanceOptions Default => new()
    {
        BatchSize = 1000,
        MaxCacheEntries = 10000,
        CacheExpirationHours = 2,
        MaxMemoryMB = 2048,
        MemoryThreshold = 0.8,
        LiteDbCacheSize = 5000,
        MaxDegreeOfParallelism = 4,
        EnableApiRetry = true,
        MaxApiRetries = 3,
        ApiTimeoutSeconds = 300,
        JsonBufferSize = 65536,
        CacheCleanupFrequencyMinutes = 15,
        EnablePerformanceLogging = true,
        EnableMemoryMonitoring = true,
        AutoDeleteObsoleteEntities = true
    };

    /// <summary>
    /// Configuration pour petite volumétrie (< 5K total)
    /// Plus économe en mémoire
    /// </summary>
    public static PerformanceOptions LowVolume => new()
    {
        BatchSize = 500,
        MaxCacheEntries = 5000,
        CacheExpirationHours = 4,
        MaxMemoryMB = 1024,
        MemoryThreshold = 0.75,
        LiteDbCacheSize = 2500,
        MaxDegreeOfParallelism = 2,
        EnableApiRetry = true,
        MaxApiRetries = 3,
        ApiTimeoutSeconds = 180,
        JsonBufferSize = 32768,
        CacheCleanupFrequencyMinutes = 30,
        EnablePerformanceLogging = false,
        EnableMemoryMonitoring = true,
        AutoDeleteObsoleteEntities = true
    };

    /// <summary>
    /// Configuration pour haute volumétrie (> 30K total)
    /// Optimisé pour la vitesse
    /// </summary>
    public static PerformanceOptions HighVolume => new()
    {
        BatchSize = 2000,
        MaxCacheEntries = 20000,
        CacheExpirationHours = 1,
        MaxMemoryMB = 4096,
        MemoryThreshold = 0.85,
        LiteDbCacheSize = 10000,
        MaxDegreeOfParallelism = 8,
        EnableApiRetry = true,
        MaxApiRetries = 5,
        ApiTimeoutSeconds = 600,
        JsonBufferSize = 131072, // 128KB
        CacheCleanupFrequencyMinutes = 10,
        EnablePerformanceLogging = true,
        EnableMemoryMonitoring = true,
        AutoDeleteObsoleteEntities = true
    };
}
