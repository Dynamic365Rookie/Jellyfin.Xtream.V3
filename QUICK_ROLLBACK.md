# ?? ROLLBACK RAPIDE - Guide Minimal

## ?? Situation

```
? Commit poussé sur: Azure DevOps (par erreur)
? Commit doit être sur: GitHub
```

---

## ?? Solution Rapide (3 étapes)

### 1?? Exécuter le Script
```powershell
.\rollback-and-push-github.ps1
```

### 2?? Répondre aux Questions
- ? Confirmer le rollback
- ? Entrer l'URL GitHub
- ? Choisir la branche GitHub
- ? Confirmer le push

### 3?? Vérifier
```bash
# Vérifier Azure DevOps (le commit doit être annulé)
git log origin/MIV_DEV01_TestPush --oneline -1

# Vérifier GitHub (le commit doit être présent)
git log github/main --oneline -1
```

---

## ?? Avant de Commencer

- [ ] URL GitHub prête
- [ ] Authentification GitHub configurée
- [ ] Vous êtes dans le bon répertoire

---

## ? Résultat Attendu

```
? Azure DevOps:  Commit ANNULÉ
? GitHub:        Commit PRÉSENT
```

---

**Exécutez le script et suivez les instructions ! ??**
