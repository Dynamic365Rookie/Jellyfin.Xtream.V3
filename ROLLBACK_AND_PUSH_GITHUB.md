# ?? Rollback et Push vers GitHub - Guide Complet

## ?? Objectif

Annuler le dernier commit pushé sur Azure DevOps et le repousser sur GitHub à la place.

---

## ?? Situation Actuelle

### Commit Accidentellement Pushé sur Azure DevOps
```
Commit Hash: 1506ae8
Message:    feat: Optimisations majeures de performance pour haute volumétrie
Branche:    MIV_DEV01_TestPush
Remote:     origin (Azure DevOps)
```

### État Souhaité
```
Remote primaire: GitHub (au lieu d'Azure DevOps)
Commit présent sur: GitHub
Commit absent de: Azure DevOps
```

---

## ? Étapes à Suivre

### Option 1: Script Automatique (Recommandé) ?

```powershell
# Dans PowerShell, exécutez:
cd "C:\Users\mvanderheyden_w\source\repos\Projects\Jellyfin.Xtream.V2\Jellyfin.Xtream.V2"
.\rollback-and-push-github.ps1
```

**Le script va:**
1. ? Vérifier l'état du repository
2. ? Afficher le commit à annuler
3. ? Demander confirmation
4. ? Annuler le commit localement
5. ? Force push pour annuler sur Azure DevOps
6. ? Configurer GitHub comme remote
7. ? Pousser vers GitHub
8. ? Afficher un résumé

---

### Option 2: Commandes Manuelles

#### Étape 1: Annuler le Commit Localement
```bash
cd "C:\Users\mvanderheyden_w\source\repos\Projects\Jellyfin.Xtream.V2\Jellyfin.Xtream.V2"

# Annuler le dernier commit (garde les fichiers modifiés)
git reset --soft HEAD~1
```

#### Étape 2: Annuler le Commit sur Azure DevOps (Force Push)
```bash
# ?? ATTENTION: Cela est irréversible - assurez-vous que c'est bien ce que vous voulez
git push origin MIV_DEV01_TestPush --force-with-lease
```

#### Étape 3: Ajouter GitHub comme Remote
```bash
# Ajouter GitHub comme remote
git remote add github https://github.com/votre-username/Jellyfin.Xtream.V2.git

# Vérifier que le remote est ajouté
git remote -v
```

#### Étape 4: Pousser vers GitHub
```bash
# Pousser vers GitHub
git push github main --set-upstream

# Ou si vous préférez une branche de feature:
git push github feature/optimisations-performance --set-upstream
```

---

## ?? Prérequis

### Avant de Commencer

1. **URL du Repository GitHub**
   ```
   Format: https://github.com/votre-username/Jellyfin.Xtream.V2.git
   ```

2. **Authentification GitHub**
   - SSH configurée, OU
   - Token personnel créé, OU
   - Username/Password

3. **Confirmation du Rollback**
   - Vous êtes sûr d'annuler le commit sur Azure DevOps
   - Vous avez l'intention de pousser vers GitHub

---

## ?? Informations Requises

Le script va demander:

### 1. URL GitHub
```
Entrez l'URL de votre repository GitHub
Exemple: https://github.com/mvanderheyden/Jellyfin.Xtream.V2.git
```

### 2. Branche de Destination
```
Quelle branche GitHub voulez-vous utiliser?
Exemples:
  - main (branche principale)
  - develop (développement)
  - feature/performance-optimization (feature branch)
```

---

## ?? Exécution du Script

### Lancer le Script
```powershell
.\rollback-and-push-github.ps1
```

### Suivre les Étapes
Le script affichera:
1. ? État actuel du repository
2. ? Confirmation du rollback
3. ? Statut après annulation
4. ? Demande de l'URL GitHub
5. ? Configuration du remote
6. ? Sélection de la branche
7. ? Confirmation finale du push
8. ? Résumé du succès

---

## ?? Points Importants

### Force Push sur Azure DevOps
```
?? ATTENTION:
- Cela va ANNULER le commit sur la branche distante Azure DevOps
- C'est irréversible
- Assurez-vous que c'est bien ce que vous voulez faire
```

### Authentification GitHub
```
Si vous avez une erreur d'authentification:
1. Vérifiez votre URL GitHub
2. Vérifiez que vous avez un token SSH ou personnel
3. Si SSH, assurez-vous que la clé est ajoutée à votre agent
4. Si HTTPS, stockez vos credentials Git
```

### Branche de Destination
```
Conseil:
- Si GitHub n'existe pas encore, créez-le d'abord
- Si vous voulez une branche séparée, nommez-la clairement
- Exemple: feature/performance-optimizations
```

---

## ?? Ce Que Fait Chaque Étape

### Étape 1: Annulation Locale
```bash
git reset --soft HEAD~1

Résultat:
- Commit annulé localement
- Fichiers restent en staging
- HEAD pointe maintenant sur le commit précédent
```

### Étape 2: Force Push Azure DevOps
```bash
git push origin MIV_DEV01_TestPush --force-with-lease

Résultat:
- Le commit est annulé sur Azure DevOps aussi
- La branche est maintenant synchronisée avec la version locale
- Le commit est irrécupérable sur Azure DevOps
```

### Étape 3: Configuration GitHub
```bash
git remote add github https://github.com/.../Jellyfin.Xtream.V2.git

Résultat:
- Nouveau remote 'github' ajouté
- 'origin' pointe toujours sur Azure DevOps
- 'github' pointe sur GitHub
```

### Étape 4: Push vers GitHub
```bash
git push github main --set-upstream

Résultat:
- Les commits sont poussés vers GitHub
- Le tracking upstream est configuré
- Vous pouvez ensuite faire `git push` directement
```

---

## ?? Vérifier le Résultat

### Après l'Exécution

#### 1. Vérifier Azure DevOps
```bash
# Le commit ne doit plus être visible
git log origin/MIV_DEV01_TestPush --oneline -5
```

#### 2. Vérifier GitHub
```
Allez sur: https://github.com/votre-username/Jellyfin.Xtream.V2
Vérifiez que le commit est présent dans la branche cible
```

#### 3. Vérifier les Remotes
```bash
git remote -v
# Doit afficher:
# origin  https://...Azure DevOps...
# github  https://github.com/.../Jellyfin.Xtream.V2.git
```

#### 4. Vérifier l'État Local
```bash
git status
# Doit afficher les fichiers modifiés (commit annulé)
```

---

## ?? Scenario Complet d'Exécution

```
1. Lancer le script
   ?
2. Voir l'état actuel
   ?? Commit Hash: 1506ae8
   ?? Message: feat: Optimisations majeures...
   ?? Branch: MIV_DEV01_TestPush
   ?
3. Confirmer le rollback
   ?? "Êtes-vous sûr? (O/N)"
   ?
4. Annulation locale
   ? Commit annulé
   ? Fichiers en staging
   ?
5. Force push Azure DevOps
   ? Commit annulé sur Azure
   ?
6. Entrer URL GitHub
   ?? https://github.com/votre-user/Jellyfin.Xtream.V2.git
   ?
7. Remote configuré
   ? github remote ajouté
   ?
8. Choisir branche GitHub
   ?? main / develop / feature-branch
   ?
9. Confirmer le push
   ?? "Êtes-vous prêt? (O/N)"
   ?
10. Push vers GitHub
    ? Succès!
    ?
11. Résumé final
    ? Commit annulé sur Azure DevOps
    ? Commit pushé sur GitHub
```

---

## ?? Dépannage

### Erreur: "fatal: refusing to merge unrelated histories"
```
Solution:
git pull github main --allow-unrelated-histories
```

### Erreur: "Permission denied (publickey)"
```
Solution (SSH):
- Vérifiez que votre clé SSH est ajoutée
- ssh-add ~/.ssh/id_rsa
```

### Erreur: "404 Not Found"
```
Solution:
- Vérifiez que l'URL GitHub est correcte
- Assurez-vous que le repository existe
- Vérifiez que vous avez accès au repository
```

### Erreur: "Pre-receive hook declined"
```
Solution:
- Les pushes peuvent être restreints sur GitHub
- Vérifiez les règles de protection de branche
- Utilisez une autre branche ou créez une PR
```

---

## ? Checklist Avant Exécution

- [ ] Vous avez l'URL du repository GitHub
- [ ] Vous savez quelle branche GitHub utiliser
- [ ] Vous avez l'authentification GitHub configurée
- [ ] Vous êtes sûr d'annuler le commit sur Azure DevOps
- [ ] Vous avez lu la section "Points Importants"
- [ ] Vous êtes dans le bon répertoire

---

## ?? Après l'Exécution

### Prochaines Étapes
1. ? Vérifier que le commit est sur GitHub
2. ? Vérifier que le commit est annulé sur Azure DevOps
3. ? Mettre à jour votre fichier `.git/config` si nécessaire
4. ? Configurer GitHub comme remote primaire (optionnel)

### Faire de GitHub le Remote Primaire (Optionnel)
```bash
# Renommer les remotes
git remote rename origin azure-devops
git remote rename github origin

# Maintenant 'origin' pointe vers GitHub
git remote -v
```

---

## ?? Résultat Final Attendu

```
Azure DevOps (UrbasolarD365Finance/D365FO_S2P):
?? MIV_DEV01_TestPush
?  ?? Commit 1506ae8 ANNULÉ ?
?
GitHub (votre-username/Jellyfin.Xtream.V2):
?? main (ou votre branche)
?  ?? Commit 1506ae8 PRÉSENT ?
```

---

**Prêt à exécuter le script ? ??**

```powershell
.\rollback-and-push-github.ps1
```
