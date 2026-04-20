# Script de création automatique de release GitHub
# Ce script crée un tag et le pousse vers GitHub pour déclencher la création de release

param(
    [string]$VersionTag = "v2.0.0",
    [string]$Message = "Release version 2.0.0 - Performance Optimized",
    [string]$Remote = "github"
)

$ErrorActionPreference = "Stop"

Write-Host ""
Write-Host "????????????????????????????????????????????????????????????????????" -ForegroundColor Cyan
Write-Host "?           Script de Création de Release GitHub                   ?" -ForegroundColor Cyan
Write-Host "????????????????????????????????????????????????????????????????????" -ForegroundColor Cyan
Write-Host ""

# Vérifier que nous sommes dans un repository Git
if (-not (Test-Path ".git")) {
    Write-Host "? Erreur: Pas dans un repository Git" -ForegroundColor Red
    exit 1
}

Write-Host "?? Repository Git trouvé" -ForegroundColor Green
Write-Host ""

# Afficher les informations
Write-Host "??  Informations du tag:" -ForegroundColor Cyan
Write-Host "  Tag: $VersionTag" -ForegroundColor White
Write-Host "  Message: $Message" -ForegroundColor White
Write-Host "  Remote: $Remote" -ForegroundColor White
Write-Host ""

# Vérifier que le tag n'existe pas déjà
$existingTag = git tag -l $VersionTag 2>$null
if ($existingTag) {
    Write-Host "??  Le tag $VersionTag existe déjà localement" -ForegroundColor Yellow
    $response = Read-Host "Voulez-vous continuer et forcer? (O/N)"
    if ($response -ne 'O' -and $response -ne 'o') {
        Write-Host "Opération annulée" -ForegroundColor Yellow
        exit 0
    }
    git tag -d $VersionTag
    Write-Host "  ? Tag local supprimé" -ForegroundColor Green
}

Write-Host ""
Write-Host "?? Étape 1: Créer le tag..." -ForegroundColor Yellow

try {
    git tag -a $VersionTag -m $Message
    Write-Host "  ? Tag créé: $VersionTag" -ForegroundColor Green
} catch {
    Write-Host "  ? Erreur lors de la création du tag: $_" -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "?? Étape 2: Vérifier l'état Git..." -ForegroundColor Yellow

$status = git status --short
if ($status) {
    Write-Host "  ??  Fichiers modifiés détectés:" -ForegroundColor Yellow
    Write-Host "$status" -ForegroundColor White
    Write-Host ""
    $response = Read-Host "Commit ces changements avant de pousser? (O/N)"
    if ($response -eq 'O' -or $response -eq 'o') {
        git add .
        git commit -m "Update: Prepare for release $VersionTag"
        Write-Host "  ? Changements committés" -ForegroundColor Green
    }
}

Write-Host ""
Write-Host "?? Étape 3: Pousser le tag vers GitHub..." -ForegroundColor Yellow

try {
    git push $Remote $VersionTag
    Write-Host "  ? Tag poussé vers GitHub" -ForegroundColor Green
} catch {
    Write-Host "  ? Erreur lors du push du tag: $_" -ForegroundColor Red
    Write-Host ""
    Write-Host "  Conseil: Essayez manuellement:" -ForegroundColor Yellow
    Write-Host "    git push $Remote $VersionTag" -ForegroundColor Cyan
    exit 1
}

Write-Host ""
Write-Host "????????????????????????????????????????????????????????????????????" -ForegroundColor Green
Write-Host "?                  ? SUCCÈS - RELEASE CRÉÉE!                      ?" -ForegroundColor Green
Write-Host "????????????????????????????????????????????????????????????????????" -ForegroundColor Green

Write-Host ""
Write-Host "?? Résumé:" -ForegroundColor Cyan
Write-Host "  Tag créé: $VersionTag" -ForegroundColor White
Write-Host "  Poussé vers: $Remote" -ForegroundColor White
Write-Host "  Status: ? Prêt pour la release" -ForegroundColor Green
Write-Host ""
Write-Host "?? Prochaines étapes:" -ForegroundColor Yellow
Write-Host "  1. Aller sur GitHub: https://github.com/Dynamic365Rookie/Jellyfin.Xtream" -ForegroundColor White
Write-Host "  2. Vérifier l'onglet Actions pour voir le workflow en cours" -ForegroundColor White
Write-Host "  3. Vérifier l'onglet Releases pour voir la release créée" -ForegroundColor White
Write-Host ""
Write-Host "? Attendez quelques secondes que le workflow s'exécute..." -ForegroundColor Yellow
Write-Host ""

# Attendre un peu avant de fermer
Write-Host "Appuyez sur une touche pour fermer..." -ForegroundColor Gray
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
