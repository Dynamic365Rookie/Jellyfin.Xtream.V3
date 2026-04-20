# Release Notes - Jellyfin Xtream V3

## Version 3.x

Ce plugin fournit une intégration Xtream Codes pour Jellyfin avec un accent sur
la performance et la stabilité.

### Principales capacités

- Synchronisation par lots pour réduire le trafic de la base de données
- Traitement parallèle pour les films, les séries et les chaînes
- Cache en mémoire avec expiration et compaction
- Prise en charge de la synchronisation incrémentielle
- Intégration de la TV en direct et de l'EPG

### Accent sur la performance

- Optimisé pour de grands catalogues
- Appels de base de données réduits grâce au regroupement
- Meilleur contrôle de la mémoire pendant la synchronisation
- Stratégie de réessai avec ralentissement exponentiel

### Remarques techniques

- Framework cible : .NET 6.0
- Stockage principal : LiteDB 5.0.21
- Compatibilité de l'API du plugin Jellyfin via MediaBrowser.Common 4.9.1.90

### Installation

```bash
dotnet restore
dotnet build
dotnet publish -c Release
```

### Documentation

- `README.md`
- `QUICKSTART.md`
- `PERFORMANCE_GUIDE.md`
- `PERFORMANCE_OPTIMIZATIONS.md`

### Remarques

- Le texte de la version est intentionnellement maintenu sûr ASCII pour éviter les problèmes d'encodage
  dans les environnements qui ne prennent pas en charge l'extended Unicode caractères.
