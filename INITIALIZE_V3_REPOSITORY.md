# ?? GUIDE COMPLET - Initialiser Jellyfin.Xtream.V3

## ?? Objectif
Initialiser un nouveau repository GitHub (https://github.com/Dynamic365Rookie/Jellyfin.Xtream.V3) avec le contenu de Jellyfin.Xtream.V2

---

## ?? ÉTAPES À SUIVRE

### Étape 1: Créer le Repository sur GitHub
```
1. Aller sur: https://github.com/Dynamic365Rookie
2. Cliquer sur "New" (créer un nouveau repository)
3. Remplir les champs:
   - Repository name: Jellyfin.Xtream.V3
   - Description: Jellyfin Xtream IPTV Plugin - Version 3 (Optimized)
   - Visibility: Public (ou Private selon vos besoins)
4. Cocher: "Add a README file" (optionnel)
5. Cliquer "Create repository"

Résultat:
   URL: https://github.com/Dynamic365Rookie/Jellyfin.Xtream.V3
```

---

### Étape 2: Initialiser Git en Local

#### Option A: Dupliquer le répertoire existant (RECOMMANDÉ)
```bash
# Créer une copie du répertoire
cd C:\Users\mvanderheyden_w\source\repos\Projects\Jellyfin.Xtream.V2\
cp -r Jellyfin.Xtream.V2 Jellyfin.Xtream.V3

# OU avec PowerShell
Copy-Item -Path "Jellyfin.Xtream.V2" -Destination "Jellyfin.Xtream.V3" -Recurse
```

#### Option B: Utiliser directement le répertoire existant
```bash
cd C:\Users\mvanderheyden_w\source\repos\Projects\Jellyfin.Xtream.V2\Jellyfin.Xtream.V2
```

---

### Étape 3: Initialiser le Repository Git

```bash
# Se placer dans le répertoire
cd C:\Users\mvanderheyden_w\source\repos\Projects\Jellyfin.Xtream.V2\Jellyfin.Xtream.V3

# Initialiser Git (si pas déjà initié)
git init

# Configurer l'utilisateur Git (si nécessaire)
git config user.name "Your Name"
git config user.email "your.email@example.com"

# Ajouter le remote GitHub
git remote add origin https://github.com/Dynamic365Rookie/Jellyfin.Xtream.V3.git

# Vérifier le remote
git remote -v
# Output: origin  https://github.com/Dynamic365Rookie/Jellyfin.Xtream.V3.git (fetch)
#         origin  https://github.com/Dynamic365Rookie/Jellyfin.Xtream.V3.git (push)
```

---

### Étape 4: Ajouter et Commiter les Fichiers

```bash
# Ajouter tous les fichiers
git add .

# Vérifier l'état
git status

# Créer le commit initial
git commit -m "Initial commit: Jellyfin.Xtream.V3 - Performance optimized version

- Based on Jellyfin.Xtream.V2 optimization work
- Includes infrastructure improvements
- Complete CI/CD automation with GitHub Actions
- Comprehensive documentation
- Target Framework: .NET 6.0"

# Renommer la branche en 'main' (si nécessaire)
git branch -M main
```

---

### Étape 5: Pousser vers GitHub

```bash
# Pousser les fichiers vers GitHub
git push -u origin main

# Output attendu:
# Enumerating objects...
# Counting objects: 100%
# Compressing objects: 100%
# Writing objects: 100%
# Total ...
# To https://github.com/Dynamic365Rookie/Jellyfin.Xtream.V3.git
#  * [new branch]      main -> main
# branch 'main' set up to track 'origin/main'.
```

---

## ?? APPROCHE DÉTAILLÉE (PowerShell)

### Commandes Complètes à Exécuter

```powershell
# 1. Créer la copie du répertoire
$sourcePath = "C:\Users\mvanderheyden_w\source\repos\Projects\Jellyfin.Xtream.V2\Jellyfin.Xtream.V2"
$destPath = "C:\Users\mvanderheyden_w\source\repos\Projects\Jellyfin.Xtream.V2\Jellyfin.Xtream.V3"

Copy-Item -Path $sourcePath -Destination $destPath -Recurse

# 2. Se placer dans le nouveau répertoire
cd $destPath

# 3. Supprimer le .git existant (si c'est une copie d'un repo)
if (Test-Path ".git") {
    Remove-Item -Path ".git" -Recurse -Force
}

# 4. Initialiser Git
git init

# 5. Configurer le remote
git remote add origin https://github.com/Dynamic365Rookie/Jellyfin.Xtream.V3.git

# 6. Ajouter tous les fichiers
git add .

# 7. Commiter
git commit -m "Initial commit: Jellyfin.Xtream.V3 base"

# 8. Renommer main
git branch -M main

# 9. Pousser vers GitHub
git push -u origin main
```

---

## ? ALTERNATIVE: Si le Git est Déjà Configuré

Si Jellyfin.Xtream.V2 a déjà un repository Git configuré vers Dynamic365Rookie/Jellyfin.Xtream:

```bash
# 1. Cloner depuis le repository existant
git clone https://github.com/Dynamic365Rookie/Jellyfin.Xtream.git Jellyfin.Xtream.V3

# 2. Se placer dans le nouveau répertoire
cd Jellyfin.Xtream.V3

# 3. Supprimer l'historique Git (optionnel - pour un nouveau départ)
rm -r .git

# 4. Réinitialiser Git
git init

# 5. Changer le remote
git remote add origin https://github.com/Dynamic365Rookie/Jellyfin.Xtream.V3.git

# 6. Ajouter tous les fichiers
git add .

# 7. Commiter
git commit -m "Initial commit: Jellyfin.Xtream.V3"

# 8. Pousser
git push -u origin main
```

---

## ?? ÉTAPES RECOMMANDÉES (RAPIDE)

### Commandes à Exécuter dans PowerShell

```powershell
# 1?? Copier le répertoire
Copy-Item -Path "C:\Users\mvanderheyden_w\source\repos\Projects\Jellyfin.Xtream.V2\Jellyfin.Xtream.V2" `
           -Destination "C:\Users\mvanderheyden_w\source\repos\Projects\Jellyfin.Xtream.V2\Jellyfin.Xtream.V3" `
           -Recurse

# 2?? Entrer dans le répertoire
cd "C:\Users\mvanderheyden_w\source\repos\Projects\Jellyfin.Xtream.V2\Jellyfin.Xtream.V3"

# 3?? Supprimer le .git existant si présent
if (Test-Path ".git") {
    Remove-Item -Path ".git" -Recurse -Force
    Write-Host "? Repository Git ancien supprimé"
}

# 4?? Initialiser Git
git init
Write-Host "? Repository Git initialisé"

# 5?? Configurer le remote
git remote add origin "https://github.com/Dynamic365Rookie/Jellyfin.Xtream.V3.git"
Write-Host "? Remote GitHub configuré"

# 6?? Vérifier la configuration
git remote -v
Write-Host "? Remote vérifié"

# 7?? Ajouter tous les fichiers
git add .
Write-Host "? Fichiers ajoutés ($(git status --short | wc -l) fichiers)"

# 8?? Créer le commit initial
git commit -m "Initial commit: Jellyfin.Xtream.V3 - Performance optimized version

- Based on Jellyfin.Xtream.V2 optimization work
- Includes infrastructure improvements
- Complete CI/CD automation with GitHub Actions
- Comprehensive documentation
- Target Framework: .NET 6.0"
Write-Host "? Commit créé"

# 9?? Renommer main
git branch -M main
Write-Host "? Branche renommée en 'main'"

# ?? Pousser vers GitHub
git push -u origin main
Write-Host "? Repository poussé vers GitHub"

# ? Vérifier
Write-Host ""
Write-Host "? Repository initialisé avec succès !"
Write-Host "   URL: https://github.com/Dynamic365Rookie/Jellyfin.Xtream.V3"
```

---

## ? VÉRIFICATIONS FINALES

### Sur GitHub
```
1. Aller sur: https://github.com/Dynamic365Rookie/Jellyfin.Xtream.V3
2. Vérifier que:
   ? Tous les fichiers sont présents
   ? Le commit initial est visible
   ? Le code est accessible
   ? README.md est affiché
```

### En Local
```bash
# Vérifier le remote
git remote -v
# Output: origin  https://github.com/Dynamic365Rookie/Jellyfin.Xtream.V3.git (fetch)
#         origin  https://github.com/Dynamic365Rookie/Jellyfin.Xtream.V3.git (push)

# Vérifier la branche
git branch
# Output: * main

# Vérifier l'historique
git log --oneline -1
# Output: xxxxx Initial commit: Jellyfin.Xtream.V3...
```

---

## ?? POINTS IMPORTANTS

### Avant de Commencer
```
? Créer le repository vide sur GitHub d'abord
? S'assurer d'avoir accès à https://github.com/Dynamic365Rookie
? Vérifier la connexion Internet
? Git doit être installé et configuré
```

### Fichiers à Inclure
```
? Tous les fichiers du projet Jellyfin.Xtream.V2
? .github/workflows/ (GitHub Actions)
? Documentation (.md files)
? Configuration (appsettings, etc.)
? .gitignore
```

### Fichiers À EXCLURE
```
? .git/ (ancien repository)
? bin/, obj/ (build artifacts)
? .vs/ (Visual Studio cache)
? *.user (projet files utilisateur)
? node_modules/ (s'il y en a)
```

---

## ?? DÉPANNAGE

### Erreur: "Repository already exists"
```bash
# Créer un nouveau répertoire
mkdir Jellyfin.Xtream.V3
cd Jellyfin.Xtream.V3
```

### Erreur: "Permission denied"
```bash
# Vérifier les credentials GitHub
# Utiliser SSH ou token personnel si HTTPS ne fonctionne pas
```

### Erreur: "Remote already exists"
```bash
# Supprimer le remote existant
git remote remove origin

# Ajouter le nouveau
git remote add origin https://github.com/Dynamic365Rookie/Jellyfin.Xtream.V3.git
```

---

## ?? SUPPORT

### Ressources
```
GitHub Docs:     https://docs.github.com/en/repositories/creating-and-managing-repositories
Git Docs:        https://git-scm.com/doc
GitHub Actions:  https://docs.github.com/en/actions
```

---

**Date**: 2024  
**Status**: ? Guide Complet  
**Objectif**: Initialiser Jellyfin.Xtream.V3

**Commencez par créer le repository vide sur GitHub, puis exécutez les commandes PowerShell ! ??**
