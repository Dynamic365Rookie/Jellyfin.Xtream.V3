# Guide de Diagnostic — Logos & EPG Jellyfin.Xtream

## 🔧 Comment Utiliser les Outils de Diagnostic

### Version requise
**v3.9.4+** contient les endpoints de diagnostic

### Accès aux endpoints
Remplace `jellyfin:8096` par ton adresse Jellyfin :
```
http://jellyfin:8096/Xtream/Debug/config
http://jellyfin:8096/Xtream/Debug/channels/icons
http://jellyfin:8096/Xtream/Debug/channels/epg
```

---

## 🖼️ **Problème 1: Logos des chaînes ne s'affichent pas**

### Diagnostic
```bash
curl "http://jellyfin:8096/Xtream/Debug/channels/icons"
```

Regarde dans la réponse JSON:

**Cas 1: Icon URL vide**
```json
{
  "IconUrl": null,
  "Issues": ["Icon URL is null or empty"],
  "SuggestedFix": null
}
```
➜ **Cause**: L'API Xtream ne retourne pas d'URL pour les logos  
➜ **Solution**: Vérifier que le champ `icon` existe dans l'API Xtream

**Cas 2: Icon URL relative**
```json
{
  "IconUrl": "/iconos/...png",
  "Issues": ["Icon is relative path: /iconos/...png"],
  "SuggestedFix": "Prefix with server URL: https://dispatcharr.kelly.ovh/iconos/...png"
}
```
➜ **Cause**: URL relative, Jellyfin ne peut pas y accéder directement  
➜ **Solution 1** (Rapide): Je peux implémenter un **proxy d'icônes** qui :
  - Transforme `/iconos/...` en `{ServerUrl}/iconos/...`
  - Sert les icônes via `/Xtream/Logo/{channelId}`  
  - Jellyfin utilisera cette URL

➜ **Solution 2**: Contacter Xtream API pour utiliser des URLs absolues

**Cas 3: Icon URL inaccessible**
```json
{
  "HttpStatusCode": 403,
  "Issues": ["HTTP 403: Forbidden"],
  "SuggestedFix": "Icon URL requires authentication - consider using a proxy endpoint"
}
```
➜ **Cause**: URL protégée par authentification  
➜ **Solution**: Activer le proxy d'icônes (Solution 1)

### Fix Recommandé: Proxy d'Icônes

Je vais implémenter un endpoint qui sert les icônes localement:

```csharp
GET /Xtream/Logo/{channelId}
├── Télécharge l'icône depuis l'URL source
├── Cache localement
└── Retourne avec les bons headers
```

Ajoute à la config:
```
ImageUrl = $"/Xtream/Logo/{c.StreamId}"  // au lieu de c.Icon
```

✅ Avantages:
- Pas de proxy externe nécessaire
- Cache local = chargement rapide
- Gère l'authentification automat.
- URLs relatives converties en absolues

---

## 📺 **Problème 2: EPG - Chaînes visibles mais pas de programmes**

### Diagnostic
```bash
curl "http://jellyfin:8096/Xtream/Debug/channels/epg"
```

Regarde dans la réponse JSON:

**Cas 1: EpgChannelId non configuré**
```json
{
  "EpgChannelId": null,
  "Issues": ["EpgChannelId is not set (null or 0)"],
  "SuggestedFix": "Ensure Xtream API returns 'epg_channel_id' for this channel"
}
```
➜ **Cause**: Champ `epg_channel_id` absent de la réponse Xtream  
➜ **Solution**: Vérifier config Xtream API

**Cas 2: Pas de programmes (tous passés)**
```json
{
  "TotalListings": 42,
  "FutureListings": 0,
  "PastListings": 42,
  "Issues": ["No future programs in EPG (all are in the past)"]
}
```
➜ **Cause**: L'horloge système est décalée OU les données EPG sont anciennes  
➜ **Solution**:
1. Vérifier l'heure système: `date`
2. Resync les données EPG (attendre 2h ou forcer)

**Cas 3: EPG API retourne 500**
```json
{
  "HttpStatusCode": 500,
  "Issues": ["EPG API returned HTTP 500"],
  "EpgEndpoint": "https://dispatcharr.kelly.ovh/player_api.php?action=get_simple_data_table&stream_id=5930"
}
```
➜ **Cause**: Serveur EPG en erreur  
➜ **Solution**: Utiliser l'endpoint XMLTV alternatif (voir plus bas)

**Cas 4: Programmes présents mais pas d'affichage**
```json
{
  "FutureListings": 15,
  "SampleProgram": {
    "Title": "Mon Programme",
    "StartTime": "2026-05-11T20:00:00Z",
    "EndTime": "2026-05-11T22:00:00Z"
  }
}
```
➜ **Cause**: Données OK, problème de synchronisation ID  
➜ **Solution**: Les IDs de chaîne ne matchent pas entre stream et EPG

### Fix 1: Vérifier les IDs de chaîne

Commande pour vérifier les IDs:
```bash
# Récupère tous les StreamId + EpgChannelId
sqlite3 /config/xtream/xtream.db "SELECT Id, Name, EpgChannelId FROM channels LIMIT 5;"
```

Si `EpgChannelId` est NULL:
- L'API Xtream ne fournit pas `epg_channel_id`
- Ou le mapping est incorrect

### Fix 2: Implémenter le Support XMLTV (EPG alternatif)

Tu mentionnes: `https://dispatcharr.kelly.ovh/output/epg`

C'est un endpoint **XMLTV standard**. Je peux implémenter un **provider EPG alternatif**:

```csharp
// Nouvelle configuration
public string XmlTvEpgUrl { get; set; } = "https://dispatcharr.kelly.ovh/output/epg";
public bool UseXmlTvEpg { get; set; } = true;
```

**Architecture:**
```
XtreamLiveTvService.GetProgramsAsync()
├─ Si UseXmlTvEpg: XmlTvEpgProvider.GetAsync()
│  └─ Parse XMLTV depuis URL
├─ Sinon: XtreamApiClient.GetAsync() [actuellement]
└─ Fallback: essayer l'autre si le premier échoue
```

✅ Avantages:
- EPG externe peut être plus fiable que Xtream API
- Support de multiples sources EPG
- Fallback automatique en cas d'erreur

---

## 📋 Résumé des Actions Recommandées

| Problème | Action | Priorité |
|----------|--------|----------|
| Icons null/vides | Activer proxy d'icônes | 🔴 Haute |
| Icons inaccessibles (403) | Activer proxy d'icônes | 🔴 Haute |
| EPG vide | Vérifier EpgChannelId | 🟡 Moyenne |
| EPG tous passés | Vérifier heure système | 🟡 Moyenne |
| EPG 500 error | Implémenter XMLTV provider | 🟢 Basse |

---

## ✅ Mon Implémentation (v3.9.5+)

Je peux créer:

1. **Logo Proxy Endpoint**
   - `GET /Xtream/Logo/{channelId}`
   - Cache + relative URL handling
   - Estimated: 30 min

2. **XMLTV EPG Provider**
   - Parse `https://dispatcharr.kelly.ovh/output/epg`
   - Fallback strategy
   - Channel ID mapping
   - Estimated: 45 min

---

## 🧪 Teste d'abord les diagnostics!

1. Installe **v3.9.4**
2. Lance les 3 endpoints `/Debug/*`
3. Envoie-moi les réponses JSON
4. On fixe ensemble les problèmes spécifiques

---

**Besoin d'aide?** Les endpoints donnent des suggestions spécifiques à ta configuration!
