# GitHub Actions Configuration Guide

## ? Workflows Configurés

Trois workflows GitHub Actions ont été créés pour votre projet:

### 1. Build and Release (build-and-release.yml)
```yaml
Déclenché par:
  - Push sur main ou develop
  - Tags v*
  - Pull requests

Actions:
  ? Build du projet
  ? Exécution des benchmarks
  ? Publication des artefacts
  ? Tests
  ? Création de release (sur tag)
```

**Déclenchement**: À chaque push

### 2. Code Quality (code-quality.yml)
```yaml
Déclenché par:
  - Push sur main ou develop
  - Pull requests

Actions:
  ? Analyse du code
  ? CodeQL analysis (sécurité)
  ? Vérification des vulnérabilités
  ? Contrôle des dépendances
```

**Déclenchement**: À chaque push/PR

### 3. Documentation (documentation.yml)
```yaml
Déclenché par:
  - Push sur main ou develop
  - Tags v*
  - Pull requests

Actions:
  ? Vérification README.md
  ? Validation CHANGELOG.md
  ? Vérification LICENSE
  ? Validation markdown
  ? Génération documentation API
  ? Validation release notes (sur tag)
```

**Déclenchement**: À chaque push/PR/tag

---

## ?? Activation des Workflows

Les workflows sont maintenant disponibles, mais vous devez:

### 1. Pousser les fichiers GitHub Actions vers GitHub
```bash
git add .github/workflows/
git add RELEASE_NOTES.md
git add CHANGELOG.md
git commit -m "ci: Add GitHub Actions workflows for build, test, and release"
git push origin main
```

### 2. Vérifier l'Activation
Allez sur GitHub:
```
https://github.com/votre-username/Jellyfin.Xtream.V2/actions
```

Vous devriez voir:
- ? build-and-release.yml
- ? code-quality.yml
- ? documentation.yml

### 3. Créer une Version/Release
Pour déclencher la création de release:
```bash
git tag v2.0.0
git push origin v2.0.0
```

---

## ?? Fichiers de Configuration

### .github/workflows/build-and-release.yml
```
Étapes:
1. Checkout du code
2. Setup .NET 6.0
3. Restore dépendances
4. Build du projet
5. Run benchmarks
6. Publish
7. Upload artefacts
8. Tests (job séparé)
9. Création release (sur tag)
```

### .github/workflows/code-quality.yml
```
Étapes:
1. Code Analysis
   - Build avec traitement des warnings
   - Vérification des vulnérabilités

2. CodeQL Analysis
   - Analyse de sécurité GitHub native
   - Détection de patterns dangereux

3. Dependency Check
   - Scan des vulnérabilités dans les packages
```

### .github/workflows/documentation.yml
```
Étapes:
1. Documentation Check
   - Vérification README.md
   - Vérification CHANGELOG.md
   - Vérification LICENSE
   - Validation markdown

2. Release Notes Validation
   - Vérification RELEASE_NOTES.md (sur tag)

3. API Documentation
   - Génération avec docfx (optionnel)
```

---

## ?? Environnement

### Variables d'Environnement
```yaml
DOTNET_VERSION: '6.0.x'
CONFIGURATION: Release
ARTIFACT_NAME: Jellyfin.Xtream.V2
```

### Permissions
- Build: actions, contents
- CodeQL: actions, contents, security-events

---

## ?? Fichiers Supportifs Créés

### RELEASE_NOTES.md
```
- Notes pour chaque version
- Lien dans les releases GitHub
- Utilisé par le workflow release
```

### CHANGELOG.md
```
- Historique complet des changements
- Format Keep a Changelog
- Consulté par la documentation
```

---

## ?? Prochaines Étapes

### 1. Pousser les Workflows
```bash
cd "C:\Users\mvanderheyden_w\source\repos\Projects\Jellyfin.Xtream.V2\Jellyfin.Xtream.V2"
git add .github/
git add RELEASE_NOTES.md
git add CHANGELOG.md
git commit -m "ci: Add GitHub Actions workflows"
git push origin main
```

### 2. Vérifier les Workflows
Aller sur: https://github.com/votre-username/Jellyfin.Xtream.V2/actions

### 3. Créer une Release (Optionnel)
```bash
git tag v2.0.0
git push origin v2.0.0
```

### 4. Activer les Branches Protégées (Recommandé)
GitHub ? Settings ? Branches
- Requérir les vérifications GitHub Actions
- Requérir l'approbation avant merge

---

## ?? Secrets GitHub (Optionnel)

Si vous avez besoin d'authentification:

### Pour NuGet Private Feed
```
Settings ? Secrets and variables ? Actions
Ajouter: NUGET_AUTH_TOKEN
```

### Pour Jira/Slack Integration
```
Settings ? Secrets and variables ? Actions
Ajouter: JIRA_TOKEN, SLACK_WEBHOOK
```

---

## ?? Status des Workflows

### Statut de Build
Badge à ajouter au README:
```markdown
[![Build and Release](https://github.com/votre-username/Jellyfin.Xtream.V2/actions/workflows/build-and-release.yml/badge.svg)](https://github.com/votre-username/Jellyfin.Xtream.V2/actions)
```

### Statut de Code Quality
```markdown
[![Code Quality](https://github.com/votre-username/Jellyfin.Xtream.V2/actions/workflows/code-quality.yml/badge.svg)](https://github.com/votre-username/Jellyfin.Xtream.V2/actions)
```

---

## ?? Dépannage

### "Workflow not triggered"
**Solution**: Pousser `.github/workflows/*.yml` vers le repo

### "Build failed"
**Vérifier**:
1. .NET 6.0 SDK disponible
2. Dépendances restaurées
3. Code compile localement

### "Release failed"
**Vérifier**:
1. RELEASE_NOTES.md existe
2. Tag au format v*
3. Permissions GITHUB_TOKEN

---

## ?? Documentation

Pour plus d'infos:
- GitHub Actions: https://docs.github.com/en/actions
- .NET Workflows: https://docs.github.com/en/actions/using-workflows
- CodeQL: https://codeql.github.com/

---

## ? Fonctionnalités Activées

- ? Build automatique sur push
- ? Tests automatiques
- ? Analyse de code
- ? Scan de sécurité (CodeQL)
- ? Vérification des vulnérabilités
- ? Validation documentation
- ? Création automatique de releases
- ? Upload d'artefacts

---

**Status**: ? Workflows Configurés et Prêts  
**Action Requise**: Pousser les fichiers `.github/` vers GitHub
