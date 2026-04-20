# Guide de Test - Chargement des Données Xtream

## 📋 Vue d'ensemble

Ce guide explique comment tester le chargement des données depuis un serveur Xtream Codes.

## 🧪 Tests Unitaires

### Exécuter la suite de tests complète

```csharp
var logger = LoggerFactory.Create(b => b.AddConsole())
    .CreateLogger<XtreamDataLoadingTests>();
var tests = new XtreamDataLoadingTests(logger);

// Exécute tous les tests
tests.RunAllTests();
```

### Tests individuels disponibles

1. **TestMovieDeserialization** - Vérifie la désérialisation des films Xtream
2. **TestSeriesDeserialization** - Vérifie la désérialisation des séries
3. **TestChannelDeserialization** - Vérifie la désérialisation des chaînes
4. **TestArrayDeserialization** - Vérifie la désérialisation des listes (réponse API réelle)
5. **TestEmptyArrayDeserialization** - Vérifie la gestion des réponses vides
6. **TestUnixTimestampConversion** - Vérifie la conversion des timestamps Unix
7. **TestConfigurationValidation** - Vérifie la validation de configuration

## ✅ Tests de Validation

### 1. Validation de Configuration

```csharp
var validator = new XtreamSyncValidator(apiClient, logger);

var result = validator.ValidateConfiguration(
    baseUrl: "http://example.xtream.local",
    username: "user123",
    password: "password123"
);

if (!result.IsValid)
{
    foreach (var error in result.Errors)
    {
        logger.LogError("Erreur: {Error}", error);
    }
}
```

**Validation effectuée:**
- ✓ URL n'est pas vide
- ✓ Username n'est pas vide
- ✓ Password n'est pas vide
- ✓ URL est un format URL valide

### 2. Test de Connectivité

```csharp
var connectivityResult = await validator.TestConnectivityAsync(
    baseUrl,
    username,
    password,
    cancellationToken
);

if (!connectivityResult.IsValid)
{
    logger.LogError("Impossible de se connecter au serveur Xtream");
}
```

**Teste:**
- ✓ Serveur est accessible
- ✓ Authentification est valide
- ✓ Timeout de connexion (10 secondes)

### 3. Validation des Endpoints

```csharp
var endpointsResult = await validator.ValidateEndpointsAsync(
    baseUrl,
    username,
    password,
    cancellationToken
);
```

**Valide les endpoints:**
- ✓ `/player_api.php?action=get_vod_streams` (Films)
- ✓ `/player_api.php?action=get_series` (Séries)
- ✓ `/player_api.php?action=get_live_streams` (Chaînes)

## 🔄 Test d'Intégration Complet

### Workflow complet avec validation

```csharp
var example = new XtreamDataLoadingIntegrationExample(
    logger,
    validator,
    syncService
);

await example.RunCompleteWorkflowAsync(
    serverUrl: "http://example.xtream.local:8080",
    username: "user123",
    password: "password123",
    ct: cancellationToken
);
```

**Étapes:**
1. Valide la configuration
2. Teste la connectivité
3. Valide les endpoints
4. Lance la synchronisation des données

### Validation rapide

```csharp
bool isValid = await example.QuickValidateAsync(
    serverUrl,
    username,
    password,
    cancellationToken
);
```

## 📊 Exemple de Réponse Xtream

### Film (Movies)

```json
{
    "stream_id": 123456,
    "name": "The Matrix",
    "image": "http://example.com/matrix.jpg",
    "rating": "8.7",
    "rating_5based": "4.35",
    "plot": "A hacker discovers the truth about reality",
    "duration": "2h 16m",
    "year": 1999,
    "genre": "Action|Sci-Fi",
    "country": "United States",
    "director": "Lana Wachowski, Lilly Wachowski",
    "writer": "Lana Wachowski, Lilly Wachowski",
    "actor": "Keanu Reeves, Laurence Fishburne",
    "category_id": 5,
    "category_name": "Movies",
    "added": 1640000000,
    "last_modified": 1650000000
}
```

### Série (Series)

```json
{
    "series_id": 789012,
    "name": "Breaking Bad",
    "image": "http://example.com/breakingbad.jpg",
    "rating": "9.5",
    "rating_5based": "4.75",
    "plot": "A chemistry teacher turns to manufacturing methamphetamine",
    "year": 2008,
    "genre": "Crime|Drama|Thriller",
    "category_id": 6,
    "category_name": "Series",
    "episodes_count": 62,
    "seasons_count": 5,
    "added": 1640000000,
    "last_modified": 1650000000
}
```

### Chaîne (LiveStreams)

```json
{
    "stream_id": 345678,
    "name": "France 2",
    "icon": "http://example.com/france2.png",
    "category_id": 1,
    "category_name": "France",
    "epg_channel_id": 101,
    "num": 2,
    "language": "FR",
    "added": 1640000000
}
```

## 🚨 Handling des Erreurs Courants

### Erreur: "Server URL is invalid"

**Cause:** URL mal formée
**Solution:** Utilisez un URL valide (ex: `http://example.com` ou `https://example.com:8080`)

### Erreur: "Cannot reach Xtream API"

**Causes possibles:**
- Serveur Xtream est offline
- Firewall bloque la connexion
- URL du serveur est incorrecte

**Solution:** Vérifiez la connectivité avec `ping` ou `curl`

### Erreur: "API returned null response - credentials may be invalid"

**Cause:** Username/password incorrects
**Solution:** Vérifiez vos identifiants Xtream

### Erreur: "Connectivity test timed out"

**Cause:** Serveur répond lentement
**Solution:** Vérifiez la latence réseau ou augmentez le timeout

## 📝 Configuration pour le Plugin Jellyfin

Pour intégrer dans le plugin Jellyfin:

```csharp
// Dans plugin startup/registration
public void ConfigureServices(IServiceCollection services)
{
    // Enregistrer les services Xtream
    XtreamDataLoadingConfiguration.ConfigureXtreamServices(services);
}

// Dans un controller/endpoint
public class XtreamSyncController
{
    [HttpPost("validate")]
    public async Task<IActionResult> ValidateAndSync(
        [FromBody] XtreamConnectionRequest request,
        [FromServices] XtreamDataLoadingIntegrationExample example)
    {
        try
        {
            await example.RunCompleteWorkflowAsync(
                request.ServerUrl,
                request.Username,
                request.Password
            );
            return Ok("Synchronization completed successfully");
        }
        catch (Exception ex)
        {
            return BadRequest($"Sync failed: {ex.Message}");
        }
    }
}
```

## ✨ Améliorations Apportées

✅ Modèles enrichis avec tous les champs Xtream  
✅ Convertisseurs JSON personnalisés pour mapping automatique  
✅ Validation pré-sync obligatoire  
✅ Tests unitaires complets  
✅ Exemple d'intégration  
✅ Gestion robuste des erreurs  

## 🎯 Prochaines Étapes

1. Exécuter les tests unitaires avec vos données réelles
2. Configurer la validation dans le plugin
3. Intégrer le workflow complet dans la synchronisation
4. Ajouter des logs pour monitoring en production
