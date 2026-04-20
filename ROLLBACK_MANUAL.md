# ?? ROLLBACK MANUEL - Commandes Git

## ?? Situation Actuelle

```
Commit à annuler: 1506ae8
Branch locale: MIV_DEV01_TestPush
Remote Azure DevOps: origin
Remote GitHub: À configurer
```

---

## ?? Étapes Manuelles

### Étape 1: Vérifier l'État
```bash
cd "C:\Users\mvanderheyden_w\source\repos\Projects\Jellyfin.Xtream.V2\Jellyfin.Xtream.V2"

# Afficher le dernier commit
git log --oneline -1
# Output: 1506ae8 feat: Optimisations majeures de performance pour haute volumétrie

# Afficher les remotes
git remote -v
# Output: origin  https://UrbasolarD365Finance@dev.azure.com/...
```

---

### Étape 2: Annuler le Commit Localement
```bash
# Annuler le dernier commit SANS supprimer les fichiers
git reset --soft HEAD~1

# Vérifier le statut
git status
# Les fichiers doivent être en "Changes to be committed"
```

---

### Étape 3: Annuler le Commit sur Azure DevOps (Force Push)
```bash
# ?? ATTENTION: Ceci est irréversible

# Option 1: Force push avec lease (plus sûr)
git push origin MIV_DEV01_TestPush --force-with-lease

# Option 2: Force push simple (plus agressif)
git push origin MIV_DEV01_TestPush --force
```

**Résultat attendu:**
```
? Master ... MIV_DEV01_TestPush (forced update)
```

---

### Étape 4: Configurer GitHub comme Remote
```bash
# Ajouter GitHub comme remote
# Remplacez 'votre-username' par votre nom d'utilisateur GitHub
git remote add github https://github.com/votre-username/Jellyfin.Xtream.V2.git

# Ou avec SSH (si configuré)
git remote add github git@github.com:votre-username/Jellyfin.Xtream.V2.git

# Vérifier les remotes
git remote -v
# Output doit afficher:
# origin  https://UrbasolarD365Finance@dev.azure.com/...
# github  https://github.com/votre-username/...
```

---

### Étape 5: Pousser vers GitHub

#### Option A: Pousser vers la branche 'main'
```bash
git push github main --set-upstream
# ou simplement
git push -u github main
```

#### Option B: Pousser vers une branche 'develop'
```bash
git push github develop --set-upstream
```

#### Option C: Pousser vers une branche feature
```bash
git push github feature/performance-optimizations --set-upstream
```

---

## ?? Scénario Complet (Copier-Coller)

```bash
# 1. Aller dans le bon répertoire
cd "C:\Users\mvanderheyden_w\source\repos\Projects\Jellyfin.Xtream.V2\Jellyfin.Xtream.V2"

# 2. Vérifier l'état
git log --oneline -1
git status

# 3. Annuler le commit localement
git reset --soft HEAD~1

# 4. Annuler le commit sur Azure DevOps
git push origin MIV_DEV01_TestPush --force-with-lease

# 5. Ajouter GitHub comme remote
git remote add github https://github.com/votre-username/Jellyfin.Xtream.V2.git

# 6. Pousser vers GitHub
git push github main --set-upstream

# 7. Vérifier le résultat
git remote -v
git log --oneline -5
```

---

## ? Vérifier le Résultat

### Vérifier Azure DevOps
```bash
# Le commit doit être annulé
git log origin/MIV_DEV01_TestPush --oneline -1
# Output: 6282082 Ajout de la solution et des projets Apsia/Urbasolar
# (pas le 1506ae8)
```

### Vérifier GitHub
```bash
# Le commit doit être présent
git log github/main --oneline -1
# Output: 1506ae8 feat: Optimisations majeures de performance pour haute volumétrie
```

### Vérifier les Remotes
```bash
git remote -v
# Output doit afficher:
# github  https://github.com/...Jellyfin.Xtream.V2.git (fetch)
# github  https://github.com/...Jellyfin.Xtream.V2.git (push)
# origin  https://UrbasolarD365Finance@dev.azure.com/... (fetch)
# origin  https://UrbasolarD365Finance@dev.azure.com/... (push)
```

---

## ?? Messages d'Erreur Courants

### "fatal: refusing to merge unrelated histories"
```bash
# Si le repository GitHub est vide
git push github main --allow-unrelated-histories
```

### "Permission denied (publickey)"
```bash
# Si vous utilisez SSH et la clé n'est pas chargée
# Soit: Ajouter la clé SSH
ssh-add ~/.ssh/id_rsa

# Soit: Utiliser HTTPS à la place
git remote set-url github https://github.com/votre-username/Jellyfin.Xtream.V2.git
```

### "fatal: repository not found"
```bash
# Vérifier l'URL GitHub
# Doit être exactement: https://github.com/USERNAME/Jellyfin.Xtream.V2.git
# Remplacer USERNAME par votre username

git remote set-url github https://github.com/USERNAME/Jellyfin.Xtream.V2.git
```

### "Updates were rejected because the tip of your current branch is behind"
```bash
# Faire un pull d'abord
git pull github main --allow-unrelated-histories
# Puis repousser
git push github main
```

---

## ?? Alternative: Changer le Remote Primaire

Si vous voulez que GitHub soit le "origin" par défaut:

```bash
# 1. Renommer les remotes
git remote rename origin azure-devops
git remote rename github origin

# 2. Vérifier
git remote -v
# Maintenant 'origin' est GitHub

# 3. Pousser avec le nouveau remote
git push origin main

# 4. Pour Azure DevOps, utiliser:
git push azure-devops MIV_DEV01_TestPush
```

---

## ? Après le Succès

### Nettoyage (Optionnel)
```bash
# Supprimer les fichiers stagiés si vous ne les voulez plus
git reset --hard HEAD

# Ou si vous les voulez, les commiter:
git commit -m "feat: Vos optimisations"
git push origin main
```

### Vérifier le Résultat Final
```bash
# Vérifier que tout est synchronisé
git status
# Output: On branch MIV_DEV01_TestPush
#         nothing to commit, working tree clean

# Vérifier l'historique
git log --oneline -5
git remote -v
```

---

## ?? Résumé des Étapes

| Étape | Commande | Objectif |
|-------|----------|----------|
| 1 | `git reset --soft HEAD~1` | Annuler commit local |
| 2 | `git push origin MIV_DEV01_TestPush --force-with-lease` | Annuler sur Azure |
| 3 | `git remote add github https://github.com/.../Jellyfin.Xtream.V2.git` | Ajouter GitHub |
| 4 | `git push github main --set-upstream` | Pousser vers GitHub |

---

**Prêt ? Exécutez les commandes ci-dessus ! ??**
