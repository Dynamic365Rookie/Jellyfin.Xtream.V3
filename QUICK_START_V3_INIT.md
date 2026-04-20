# ?? QUICK START - Initialiser Jellyfin.Xtream.V3

## ? ÉTAPES RAPIDES (5 minutes)

### 1?? Créer le Repository Vide sur GitHub
```
1. Aller sur: https://github.com/Dynamic365Rookie
2. Cliquer "New"
3. Repository name: Jellyfin.Xtream.V3
4. Visibility: Public
5. Cliquer "Create repository"

Résultat: https://github.com/Dynamic365Rookie/Jellyfin.Xtream.V3
```

---

### 2?? Exécuter le Script PowerShell (AUTOMATISÉ)

```powershell
# Ouvrir PowerShell dans le répertoire du projet
cd "C:\Users\mvanderheyden_w\source\repos\Projects\Jellyfin.Xtream.V2\Jellyfin.Xtream.V2"

# Exécuter le script d'initialisation
.\initialize-v3-repository.ps1

# Le script va:
# ? Créer Jellyfin.Xtream.V3
# ? Initialiser Git
# ? Configurer GitHub
# ? Créer le commit initial
# ? Pousser vers GitHub
```

---

### 3?? OU Exécuter Manuellement (Commandes)

```powershell
# Se placer dans le répertoire V2
cd "C:\Users\mvanderheyden_w\source\repos\Projects\Jellyfin.Xtream.V2"

# Copier le répertoire
Copy-Item -Path "Jellyfin.Xtream.V2" -Destination "Jellyfin.Xtream.V3" -Recurse

# Entrer dans V3
cd "Jellyfin.Xtream.V3"

# Supprimer le .git existant
if (Test-Path ".git") {
    Remove-Item -Path ".git" -Recurse -Force
}

# Initialiser Git
git init
git remote add origin "https://github.com/Dynamic365Rookie/Jellyfin.Xtream.V3.git"
git add .
git commit -m "Initial commit: Jellyfin.Xtream.V3"
git branch -M main
git push -u origin main
```

---

## ? VÉRIFICATION

### Après l'exécution

1. **Vérifier sur GitHub**
```
https://github.com/Dynamic365Rookie/Jellyfin.Xtream.V3
? Tous les fichiers doivent être présents
? README.md doit être affiché
```

2. **Vérifier en Local**
```bash
cd Jellyfin.Xtream.V3
git log --oneline -1
git remote -v
```

---

## ?? OPTION RECOMMANDÉE

**Exécuter le script PowerShell:**
```powershell
.\initialize-v3-repository.ps1
```

C'est le plus simple et le plus sûr ! ??

---

**Besoin d'aide? Voir INITIALIZE_V3_REPOSITORY.md pour le guide complet**
