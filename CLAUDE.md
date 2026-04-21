# CLAUDE.md — Autonomous Agent Design for Jellyfin.Xtream.V3

## Executive Summary

- **Repository**: `dynamic365rookie/Jellyfin.Xtream.V3` (GitHub, branch `main`)
- **Stack**: C# / .NET 9.0, Jellyfin plugin SDK 10.11.x, LiteDB, xUnit (tests)
- **Architecture**: Domain-driven layered (Api / Domain / Infrastructure / Services / JellyfinIntegration)
- **Agent role**: Generate code, run quality gates, commit via Conventional Commits, open PRs, iterate on CI feedback
- **Quality target**: ≥80% test coverage, zero critical SAST findings, all lints green before merge
- **Safety**: PR-only mode (never push directly to `main`), whitelisted paths, no secrets in repo

---

## 1. Prerequisites Checklist

### Access & Permissions
- [ ] GitHub PAT or GitHub App with scopes: `repo`, `workflow`, `write:packages` (least privilege)
- [ ] PAT stored as repository secret `GH_TOKEN` — never in code
- [ ] Branch protection on `main`: require PR, ≥1 approval, status checks must pass, no force-push
- [ ] CODEOWNERS file if team review routing is needed

### Runtime Environment
- [ ] .NET 9.0 SDK installed (`dotnet --version` ≥ 9.0.0)
- [ ] Git CLI ≥ 2.40
- [ ] Python 3.10+ (for pre-commit hooks and any Python tooling)
- [ ] `dotnet tool restore` for local tools (format, coverage)
- [ ] Optional: Docker for reproducible builds

### Governance
- [ ] Branch protection rules enforced via GitHub API or UI
- [ ] Required status checks: `build`, `test`, `lint`, `coverage`
- [ ] Conventional Commits enforced (commitlint or CI check)
- [ ] Signed commits recommended (GPG or SSH signing)

### Observability
- [ ] GitHub Actions workflow run logs (built-in)
- [ ] Code coverage reports uploaded as artifacts + PR comments
- [ ] CodeQL and dependency scanning alerts enabled

### Security
- [ ] CodeQL enabled for C# (already configured)
- [ ] `dotnet list package --vulnerable` in CI (already configured)
- [ ] No `.env`, `.pfx`, or credential files tracked (enforced via `.gitignore`)
- [ ] Secret scanning enabled on repository settings

---

## 2. Agent Design and Workflow

### Role & Capabilities
The agent can:
1. **Generate code** — new features, bug fixes, refactors within whitelisted paths
2. **Run tests** — execute `dotnet test` with coverage collection
3. **Format & lint** — run `dotnet format` and analyzers
4. **Commit** — using Conventional Commits (`feat:`, `fix:`, `refactor:`, `test:`, `docs:`, `chore:`)
5. **Open PRs** — via `gh pr create` with structured body
6. **Iterate on CI** — read CI results, fix failures, push fixes

### Decision Workflow

```
┌─────────────┐
│  PLAN        │  Understand task, read relevant code, design approach
│  (think)     │  Output: file list, change summary, risk assessment
└──────┬──────┘
       ▼
┌─────────────┐
│  IMPLEMENT   │  Write code changes within whitelisted paths
│  (code)      │  Follow architecture: Domain → Services → Infrastructure
└──────┬──────┘
       ▼
┌─────────────┐
│  FORMAT      │  dotnet format --verify-no-changes (then fix if needed)
│  (lint)      │  .editorconfig rules enforced
└──────┬──────┘
       ▼
┌─────────────┐
│  TEST        │  dotnet test --collect:"XPlat Code Coverage"
│  (verify)    │  Fail if coverage < 80% on changed files
└──────┬──────┘
       ▼
┌─────────────┐
│  ANALYZE     │  Static analysis (Roslyn analyzers, CodeQL local)
│  (quality)   │  Fail on any Error-severity diagnostic
└──────┬──────┘
       ▼
┌─────────────┐
│  BUILD       │  dotnet build -c Release (must succeed)
│  (package)   │
└──────┬──────┘
       ▼
┌─────────────┐
│  COMMIT      │  git add <specific files> && git commit (Conventional Commits)
│  (save)      │  Never `git add .` — explicit file list only
└──────┬──────┘
       ▼
┌─────────────┐
│  PR          │  gh pr create --title "type(scope): description"
│  (review)    │  Body: Summary, Test Plan, Breaking Changes
└──────┬──────┘
       ▼
┌─────────────┐
│  CI WATCH    │  Wait for GitHub Actions checks
│  (iterate)   │  If failed → diagnose → fix → push → repeat (max 3 iterations)
└──────┬──────┘
       ▼
┌─────────────┐
│  DONE        │  All checks green, PR ready for human review
│  (complete)  │  Auto-merge only if branch protection permits
└─────────────┘
```

### Prompting Strategy
- **System instructions**: This CLAUDE.md file serves as the system prompt
- **Chain-of-thought**: Plan step uses hidden reasoning before any code changes
- **Step-back planning**: Before modifying code, read all affected files first
- **ReAct pattern**: Observe (read code/errors) → Think (plan fix) → Act (edit/commit)

### Tool Integration
| Tool | Purpose | Command |
|------|---------|---------|
| Git CLI | Version control | `git add`, `git commit`, `git push` |
| GitHub CLI | PR/issue management | `gh pr create`, `gh pr checks` |
| dotnet CLI | Build, test, format | `dotnet build`, `dotnet test`, `dotnet format` |
| dotnet-coverage | Coverage collection | `dotnet test --collect:"XPlat Code Coverage"` |
| reportgenerator | Coverage reports | `reportgenerator -reports:**/coverage.cobertura.xml` |

### Safety Constraints
1. **Never write secrets** to any tracked file — reference `${{ secrets.NAME }}` in workflows
2. **Whitelisted paths** — agent may only modify:
   - `Api/`, `Domain/`, `Infrastructure/`, `Services/`, `JellyfinIntegration/`
   - `Configuration/`, `BackgroundTasks/`, `Observability/`
   - `Tests/` (test project)
   - `.github/workflows/`, `.github/scripts/`
   - Root config files: `*.csproj`, `*.sln`, `.editorconfig`, `Makefile`, `CLAUDE.md`
3. **Forbidden paths** — agent must never modify:
   - `.env`, `*.pfx`, `*.key`, `credentials.*`, `appsettings.*.json` with secrets
4. **PR-only mode** — never push directly to `main` or `develop`; always create a feature branch
5. **Max 3 CI retry iterations** — if still failing after 3 fix attempts, stop and report
6. **No force-push** — never use `git push --force` or `git reset --hard`

---

## 3. Repository Structure and Standards

### Target Layout
```
Jellyfin.Xtream.V3/
├── .github/
│   ├── workflows/
│   │   ├── ci.yml                    # Unified PR checks (build, test, lint, coverage)
│   │   ├── code-quality.yml          # CodeQL + dependency scanning
│   │   ├── release-tag.yml           # Tag-triggered releases
│   │   └── publish-repository.yml    # Repository manifest publishing
│   ├── scripts/
│   │   ├── generate-repository.ps1
│   │   └── extract-version-notes.ps1
│   └── CODEOWNERS
├── Api/                              # HTTP client layer (Xtream API)
│   ├── XtreamApiClient.cs
│   ├── XtreamApiEndpoints.cs
│   └── XtreamApiRateLimiter.cs
├── BackgroundTasks/                  # Jellyfin scheduled tasks
├── Configuration/                    # Plugin config, options, validators
├── Domain/                           # Core domain models (no external deps)
│   ├── Models/
│   ├── ValueObjects/
│   └── Enums/
├── Infrastructure/                   # Cross-cutting (caching, persistence, serialization)
│   ├── Caching/
│   ├── Persistence/
│   ├── Serialization/
│   ├── Monitoring/
│   └── Utilities/
├── JellyfinIntegration/              # Jellyfin SDK adapters
├── Observability/                    # Metrics, performance monitoring
├── Services/                         # Application services (business logic)
│   ├── LiveTv/
│   ├── Mapping/
│   ├── Media/
│   └── Synchronization/
├── Tests/
│   └── Jellyfin.Xtream.V3.Tests/    # Separate xUnit test project
│       ├── Api/
│       ├── Domain/
│       ├── Infrastructure/
│       ├── Services/
│       └── Jellyfin.Xtream.V3.Tests.csproj
├── Jellyfin.Xtream.V3.csproj
├── Jellyfin.Xtream.V3.sln           # Solution file (main + tests)
├── .editorconfig
├── .gitignore
├── CLAUDE.md
├── CHANGELOG.md
├── README.md
├── Makefile
├── manifest.json
├── meta.json
└── Directory.Build.props             # Shared build properties
```

### Architecture Boundaries
```
┌─────────────────────────────────────────────┐
│              JellyfinIntegration            │  ← Jellyfin SDK adapter layer
│  (LibraryUpdater, MediaSourceFactory, etc.) │
├─────────────────────────────────────────────┤
│                  Services                    │  ← Application/business logic
│  (LiveTv, Media, Synchronization, Mapping)  │
├─────────────────────────────────────────────┤
│                   Domain                     │  ← Pure domain models (no deps)
│  (Models, ValueObjects, Enums)              │
├─────────────────────────────────────────────┤
│               Infrastructure                 │  ← Technical concerns
│  (Caching, Persistence, Serialization)      │
├─────────────────────────────────────────────┤
│                    Api                       │  ← External HTTP client
│  (XtreamApiClient, RateLimiter)             │
└─────────────────────────────────────────────┘
```

**Rules**:
- `Domain` has zero project references — it defines interfaces, models, enums only
- `Services` depends on `Domain` (and optionally `Api` interfaces)
- `Infrastructure` implements `Domain` interfaces (repository pattern)
- `JellyfinIntegration` is the composition root — wires everything together
- Dependencies flow inward: Integration → Services → Domain ← Infrastructure

### Naming Conventions
- **Files**: PascalCase matching the primary type (`XtreamApiClient.cs`)
- **Namespaces**: `Jellyfin.Xtream.V3.{Layer}.{Sublayer}` (e.g., `Jellyfin.Xtream.V3.Services.Media`)
- **Interfaces**: `I` prefix (`IXtreamApiClient`, `IChannelRepository`)
- **Tests**: `{ClassUnderTest}Tests.cs` in matching folder structure
- **Commits**: Conventional Commits — `type(scope): description`
  - Types: `feat`, `fix`, `refactor`, `test`, `docs`, `chore`, `perf`, `ci`
  - Scope: layer or feature area (`api`, `domain`, `sync`, `cache`, `ci`)

### Docstring Standards
```csharp
/// <summary>
/// Brief description of the method's purpose.
/// </summary>
/// <param name="name">Description of parameter.</param>
/// <returns>Description of return value.</returns>
/// <exception cref="InvalidOperationException">When X condition occurs.</exception>
```
Only add XML docs to public API surfaces and non-obvious internal methods.

---

## 4. Build & Quality Commands

```bash
# Bootstrap
make restore          # dotnet restore + tool restore
make build            # dotnet build -c Release

# Quality
make format           # dotnet format (auto-fix)
make format-check     # dotnet format --verify-no-changes
make lint             # dotnet build -warnaserror

# Test
make test             # dotnet test with coverage
make coverage-report  # Generate HTML coverage report

# Release
make publish          # dotnet publish -c Release -o ./publish
make clean            # Remove bin/, obj/, publish/, coverage/

# All checks (what CI runs)
make ci               # restore → build → format-check → test → coverage-check
```

---

## 5. Conventional Commit Examples

```
feat(api): add health endpoint returning version from environment
fix(sync): resolve race condition in delta detection for large catalogs
refactor(infrastructure): extract repository pattern for LiteDB access
test(services): add unit tests for channel mapping service (85% coverage)
docs(readme): add quick-start section for plugin installation
chore(ci): upgrade GitHub Actions to .NET 9.0 and actions/checkout@v4
perf(cache): implement sliding expiration for memory cache entries
ci(workflows): add coverage threshold enforcement at 80%
```

---

## 6. Risks and Mitigations

| Risk | Impact | Mitigation |
|------|--------|------------|
| Secret leakage in commits | Critical | `.gitignore` blocks sensitive files; pre-commit hook scans for patterns; GitHub secret scanning enabled |
| Bad merge to main | High | Branch protection: require PR + 1 approval + all checks green; no force-push allowed |
| Flaky tests | Medium | Tests isolated (no shared state); retry logic only at CI level (`continue-on-error: false`); track flaky tests in issues |
| Agent modifies wrong files | High | Whitelist enforcement in CLAUDE.md; agent reads file before editing; PR review catches outliers |
| Coverage regression | Medium | CI fails if coverage < 80%; coverage delta reported on PR comments |
| Dependency vulnerabilities | Medium | `dotnet list package --vulnerable` in CI; Dependabot/CodeQL alerts enabled |
| CI cache poisoning | Low | Use `actions/cache` with hash-based keys; caches scoped to branch |
| Agent infinite retry loop | Medium | Max 3 CI fix iterations; agent stops and reports if still failing |
| Breaking Jellyfin ABI | High | ExcludeAssets=runtime on Jellyfin packages; version pinned in csproj; integration test validates plugin loading |
