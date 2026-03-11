# Repository Analysis

## Purpose

Issue `#7` adds the first deterministic repository analysis engine for Architecture Studio. The goal is to inspect the current workspace and produce auditable evidence that later architecture, compliance, and reporting features can reuse.

The story delivers:

- a C# repository analyzer for framework, infrastructure, CI/CD, language, and architecture-pattern signals
- a sensitive-data classifier for personal, financial, health, and child-data indicators
- a thin TypeScript transport model for extension-side orchestration
- a workspace-aware `Analyze Repository` command boundary in the extension host

## C# Ownership

Core analysis logic lives in C# under `core/ArchitectureStudio.Core/Analysis/`.

Current files:

- `RepositoryAnalysisModels.cs`
- `RepositoryAnalysisEngine.cs`

The engine takes a workspace path and returns a structured `RepositoryAnalysisResult` with:

- `signals`
- `sensitiveData`

Each item includes:

- a typed category
- confidence
- evidence
- affected paths

This keeps the output auditable and ready for downstream consumers instead of forcing later stories to scrape human-readable log messages.

## Current Heuristics

The first version intentionally uses deterministic local heuristics only. No network lookup or remote enrichment is involved.

The analyzer currently detects examples called out in `codex/studio.md`, including:

- ASP.NET Core
- Spring Boot
- React
- Angular
- Docker
- Kubernetes
- GitHub Actions
- Jenkins

It also derives:

- language signals such as C#, Java, and TypeScript
- an initial Clean Architecture signal from layer-oriented repository structure

## Sensitive Data Classification

The classifier scans repository text content for indicator patterns covering:

- personal data
- financial data
- health data
- child-data indicators

This is evidence-based classification, not final legal advice. Later compliance stories can layer policy interpretation on top of these auditable detections.

## TypeScript Boundary

The TypeScript side does not implement heuristics. It owns:

- transport types in `src/analysis/repositoryAnalysis.ts`
- the command handler that resolves the active workspace
- extension-host orchestration

The `Analyze Repository` command now targets the active workspace automatically through `getWorkspaceFolder()`. The actual analysis result is consumed through a service boundary so later interop can swap in a real process bridge without changing the command contract.

## Testing

Issue `#7` is covered by:

- `core/ArchitectureStudio.Core.Tests/RepositoryAnalysisEngineTests.cs`
  - required detections
  - sensitive data categories
  - evidence, confidence, and affected-path coverage
- `test/commands/analyzeRepositoryHandler.test.ts`
  - current-workspace targeting
  - no-workspace error handling
- `test/analysis/repositoryAnalysisArtifacts.test.ts`
  - transport and documentation artifacts

## Follow-On Work

Later stories should build on this by:

- wiring the extension to the C# engine across a real process boundary
- surfacing analysis results in the dashboard
- feeding repository evidence into compliance scoring
- exporting the analysis results in reports
