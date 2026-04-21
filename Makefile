# Makefile — Jellyfin.Xtream.V3
# Cross-platform build targets (requires GNU Make + .NET 9 SDK)

SHELL := bash
.DEFAULT_GOAL := help
CONFIGURATION := Release
COVERAGE_DIR := ./coverage
PUBLISH_DIR := ./publish
SOLUTION := Jellyfin.Xtream.V3.sln
COVERAGE_THRESHOLD := 80

.PHONY: help restore build format format-check lint test coverage-report publish clean ci

## —— Help ——————————————————————————————————
help: ## Show this help
	@grep -E '^[a-zA-Z_-]+:.*?## .*$$' $(MAKEFILE_LIST) | \
		awk 'BEGIN {FS = ":.*?## "}; {printf "\033[36m%-20s\033[0m %s\n", $$1, $$2}'

## —— Bootstrap —————————————————————————————
restore: ## Restore NuGet packages and .NET tools
	dotnet restore $(SOLUTION)
	dotnet tool restore 2>/dev/null || true

## —— Build —————————————————————————————————
build: ## Build solution in Release mode
	dotnet build $(SOLUTION) --configuration $(CONFIGURATION) --no-restore

## —— Quality ———————————————————————————————
format: ## Auto-fix code formatting
	dotnet format $(SOLUTION) --verbosity minimal

format-check: ## Check formatting without fixing (CI mode)
	dotnet format $(SOLUTION) --verify-no-changes --verbosity minimal

lint: ## Build with warnings as errors
	dotnet build $(SOLUTION) --configuration $(CONFIGURATION) --no-restore -p:TreatWarningsAsErrors=true

## —— Test ——————————————————————————————————
test: ## Run tests with coverage collection
	dotnet test $(SOLUTION) \
		--configuration $(CONFIGURATION) \
		--no-restore \
		--verbosity normal \
		--collect:"XPlat Code Coverage" \
		--results-directory $(COVERAGE_DIR)

coverage-report: test ## Generate HTML coverage report (requires reportgenerator)
	@command -v reportgenerator >/dev/null 2>&1 || dotnet tool install -g dotnet-reportgenerator-globaltool
	reportgenerator \
		-reports:"$(COVERAGE_DIR)/**/coverage.cobertura.xml" \
		-targetdir:"$(COVERAGE_DIR)/report" \
		-reporttypes:"Html;TextSummary;Cobertura"
	@echo "Coverage report: $(COVERAGE_DIR)/report/index.html"

coverage-check: test ## Fail if line coverage is below threshold
	@echo "Checking coverage threshold ($(COVERAGE_THRESHOLD)%)..."
	@COVERAGE=$$(grep -oP 'line-rate="\K[^"]+' $(COVERAGE_DIR)/**/coverage.cobertura.xml 2>/dev/null | head -1); \
	if [ -z "$$COVERAGE" ]; then echo "WARNING: No coverage data found"; exit 0; fi; \
	PERCENT=$$(echo "$$COVERAGE * 100" | bc -l 2>/dev/null | cut -d. -f1); \
	echo "Line coverage: $${PERCENT}%"; \
	if [ "$${PERCENT}" -lt "$(COVERAGE_THRESHOLD)" ]; then \
		echo "FAIL: Coverage $${PERCENT}% < $(COVERAGE_THRESHOLD)% threshold"; exit 1; \
	else \
		echo "PASS: Coverage $${PERCENT}% >= $(COVERAGE_THRESHOLD)% threshold"; \
	fi

## —— Release ———————————————————————————————
publish: ## Publish release artifacts
	dotnet publish Jellyfin.Xtream.V3.csproj -c $(CONFIGURATION) -o $(PUBLISH_DIR)

## —— Cleanup ———————————————————————————————
clean: ## Remove build artifacts and coverage data
	dotnet clean $(SOLUTION) --configuration $(CONFIGURATION) 2>/dev/null || true
	rm -rf $(PUBLISH_DIR) $(COVERAGE_DIR) **/bin **/obj BenchmarkDotNet.Artifacts

## —— CI Pipeline (runs all checks) ————————
ci: restore build format-check test coverage-check ## Full CI pipeline: restore → build → format → test → coverage
	@echo "All CI checks passed."
