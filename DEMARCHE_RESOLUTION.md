# DÉMARCHE DE RÉSOLUTION DES PROBLÈMES

## ?? RÉSUMÉ DES PROBLÈMES

### Problème 1: Plugin ne se charge pas ?
**Symptôme**: L'assembly est chargée mais le plugin n'apparaît pas dans la liste

### Problème 2: Encodage des fichiers markdown et release notes ?
**Symptôme**: Caractères spéciaux affichés comme `?` ou `??`

---

## ?? DÉMARCHE PROPOSÉE

### ÉTAPE 1: Tester v3.1.5 (EN COURS)

**Actions:**
1. Attendre que v3.1.5 soit publié (2-5 min)
2. Installer/mettre à jour dans Jellyfin
3. Redémarrer Jellyfin
4. Vérifier les logs

**Chercher dans les logs:**
```
? [INF] Loaded assembly "Jellyfin.Xtream.V3, Version=1.0.0.0..."
? [INF] Loaded plugin: "Jellyfin Xtream" "3.1.5"  ? Cette ligne!
```

**Si le plugin apparaît:** ? PROBLÈME RÉSOLU
**Si le plugin n'apparaît pas:** ? Passer à l'étape 2

---

### ÉTAPE 2: Activer les logs de debug Jellyfin

**Si v3.1.5 ne fonctionne pas:**

1. Dans Jellyfin: Dashboard ? Logs
2. Changer le niveau de log pour voir les exceptions
3. Chercher des messages d'erreur lors du chargement des plugins
4. Identifier l'exception exacte qui empêche le chargement

**Commandes pour activer debug:**
- Dashboard ? Settings ? Logging
- Niveau: Debug
- Redémarrer Jellyfin
- Vérifier les logs pour exceptions

---

### ÉTAPE 3: Analyser la cause racine

**Hypothèses à tester:**

**A. Incompatibilité de version MediaBrowser.Common**
```
Current: MediaBrowser.Common 4.9.1.90
Jellyfin: 10.11.8.0

Action: Trouver le bon package NuGet pour Jellyfin 10.11
- Chercher sur nuget.org
- Ou référencer directement les DLLs de Jellyfin
```

**B. Constructeur incorrect**
```
Current: Plugin(IApplicationPaths, IXmlSerializer)

Action: Vérifier si Jellyfin 10.11 utilise un constructeur différent
- Comparer avec plugins fonctionnels (Reports, Webhook)
- Peut-être besoin de paramètres supplémentaires
```

**C. Exception silencieuse**
```
Action: Ajouter try-catch dans constructeur
- Logger toute exception
- Identifier le point de défaillance
```

---

### ÉTAPE 4: Solutions alternatives

**Option A: Utiliser un plugin template officiel**
```bash
# Cloner le template officiel Jellyfin
git clone https://github.com/jellyfin/jellyfin-plugin-template

# Adapter notre code au template
# Garantit compatibilité avec Jellyfin 10.11
```

**Option B: Copier structure d'un plugin fonctionnel**
```
Plugins fonctionnels dans vos logs:
- Intro Skipper 1.10.11.17
- Reports 18.0.0.0
- Webhook 18.0.0.0

Action:
1. Télécharger source d'un de ces plugins
2. Comparer structure et packages
3. Adapter notre plugin
```

**Option C: Build manuel de test**
```
1. Build du plugin localement
2. Copier manuellement dans /config/plugins/
3. Tester directement
4. Voir logs détaillés
```

---

## ?? PLAN D'ACTION IMMÉDIAT

### Pour le chargement du plugin:

**1. Tester v3.1.5 ? (En cours)**
   - Release créée et poussée
   - Attendre publication
   - Installer et tester

**2. Si échec, option rapide:**
   ```
   Télécharger source du plugin Jellyfin.Xtream de Kevinjil:
   https://github.com/Kevinjil/Jellyfin.Xtream

   Comparer:
   - .csproj (packages versions)
   - Plugin.cs (structure constructor)
   - Configuration files

   Adapter notre code pour correspondre
   ```

**3. Si toujours échec, démarche avancée:**
   ```
   a) Créer un plugin ultra-minimal "Hello World"
   b) Vérifier qu'il se charge
   c) Ajouter progressivement les fonctionnalités
   d) Identifier ce qui casse
   ```

---

### Pour l'encodage:

**1. Fix temporaire (rapide):**
   ```
   Supprimer tous les émojis et caractères spéciaux
   des fichiers markdown et release notes.

   Utiliser uniquement ASCII simple.
   ```

**2. Fix permanent (correct):**
   ```powershell
   # Recréer les fichiers en UTF-8 sans BOM
   $files = @("README.md", "QUICKSTART.md", "PERFORMANCE_GUIDE.md")

   foreach ($file in $files) {
       $content = Get-Content $file -Raw
       # Remove or replace special chars
       $content = $content -replace '[????????]', '-'
       # Save as UTF-8 no BOM
       $utf8 = New-Object System.Text.UTF8Encoding $false
       [System.IO.File]::WriteAllText($file, $content, $utf8)
   }
   ```

**3. Fix des release notes dans repository.json:**
   ```
   Modifier generate-repository.ps1:
   - Nettoyer le changelog avant de l'inclure
   - Supprimer caractères spéciaux
   - Convertir emojis en texte
   - Garantir ASCII-safe
   ```

---

## ?? PROCHAINE RELEASE (v3.1.6)

### Si v3.1.5 ne fonctionne pas:

**Option 1: Cloner le plugin Kevinjil et adapter**
- Garantit compatibilité
- Packages corrects
- Structure validée

**Option 2: Plugin template officiel**
- Base propre
- Documentation officielle
- Support communauté

**Option 3: Diagnostic approfondi**
- Logs debug Jellyfin
- Reflection pour voir les plugins chargés
- Identifier l'exception exacte

---

## ? ACTIONS IMMÉDIATES RECOMMANDÉES

1. **Tester v3.1.5** (attendre 2-5 min)

2. **Si échec, activer debug logs:**
   - Jellyfin Dashboard ? Logging
   - Level: Debug
   - Chercher exceptions

3. **Comparer avec plugin fonctionnel:**
   ```bash
   # Télécharger source Intro Skipper ou autre
   # Comparer .csproj packages
   # Copier la structure qui fonctionne
   ```

4. **Fix encodage en parallèle:**
   - Supprimer émojis des markdown
   - Resauvegarder en UTF-8 sans BOM
   - Tester affichage

---

## ?? BESOIN D'AIDE?

Si aucune de ces solutions ne fonctionne:

1. Vérifiez les packages NuGet du plugin Kevinjil
2. Comparez les versions de dépendances
3. Testez avec un plugin template officiel Jellyfin
4. Activez les logs debug pour voir l'exception exacte

---

**Status**: Guide créé, v3.1.5 en cours de déploiement
**Next**: Tester v3.1.5 et suivre le plan d'action selon résultat
