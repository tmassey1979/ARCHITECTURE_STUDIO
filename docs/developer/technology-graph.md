# Technology Graph

## Purpose

Issue `#6` adds the first architecture reasoning engine for Architecture Studio. The implementation keeps the reasoning and dataset ownership in C# while exposing a thin TypeScript transport model for future extension-host orchestration.

The story delivers:

- a modular technology graph dataset under `graph/datasets/`
- a C# catalog loader for the dataset
- a compatibility and recommendation engine
- an architecture validation engine for the required rule set from `codex/studio.md`
- transport contracts in TypeScript for later process or interop boundaries

## Project Layout

- `graph/datasets/*.yml`
  - modular graph data files grouped by domain area
- `core/ArchitectureStudio.Core/Graph/TechnologyGraphCatalog.cs`
  - loads and normalizes the dataset into contract-backed graph nodes and edges
- `core/ArchitectureStudio.Core/Graph/TechnologyGraphEngine.cs`
  - evaluates stack compatibility and architecture rule violations
- `core/ArchitectureStudio.Core/Graph/TechnologyGraphModels.cs`
  - graph-specific request and result models
- `src/graph/technologyGraph.ts`
  - thin extension-side transport types

## Dataset Shape

Each YAML file contains a `nodes` array. A node carries:

- `id`
- `label`
- `category`
- optional `requires`
- optional `conflicts`
- optional `pairs_with`
- optional `recommended_with`

This lets the dataset stay text-based, easy to diff, and easy to expand without changing the engine implementation for every new technology.

Example:

```yaml
nodes:
  - id: react
    label: React
    category: Framework
    requires:
      - javascript
    pairs_with:
      - rest-api
```

## Reasoning Model

`TechnologyGraphEngine.Evaluate(...)` takes a selected stack and returns:

- normalized selected graph nodes
- missing required nodes
- explicit conflicts already present in the selected stack
- recommendations from `pairs_with` and `recommended_with`

The engine keeps rule evaluation separate from YAML loading. That separation matters because later stories will need to consume the same reasoning logic from reports, compliance, and generation features without rewriting graph persistence code.

## Architecture Validation

`TechnologyGraphEngine.ValidateArchitecture(...)` currently emits typed findings for the required first-pass rule set:

- domain referencing infrastructure implementations
- business logic in UI code
- direct database access from controllers
- missing authentication

Each finding includes:

- severity
- risk
- evidence
- remediation metadata

That shape is aligned to the shared contracts so later reporting and compliance work can reuse the same findings without adapter code.

## Dataset Coverage

The shipped dataset covers the broad curated categories called out in `codex/data.md`, including:

- frontend frameworks
- backend frameworks
- cloud platforms
- databases
- CI/CD tools
- messaging tools
- observability tools
- security tooling
- architecture patterns
- governance and regulation anchors

The current files are intentionally modular rather than collapsed into one document. That keeps additions localized and makes review simpler when the dataset expands.

## TypeScript Boundary

The TypeScript file does not implement reasoning logic. It mirrors the engine request and result shapes so the VS Code extension can:

- invoke a future C# host or process boundary
- render reasoning results in the dashboard
- keep command orchestration typed without duplicating business rules in TypeScript

This is an intentional boundary rule for the repository: TypeScript coordinates, C# decides.

## Testing

Issue `#6` is covered by:

- `core/ArchitectureStudio.Core.Tests/TechnologyGraphEngineTests.cs`
  - dataset coverage
  - required node and edge types
  - named architecture patterns
  - compatibility evaluation
  - architecture validation findings
- `test/graph/graphArtifacts.test.ts`
  - extension transport and documentation artifact presence

## Extension Follow-On

Later stories can build on this engine by:

- wiring command handlers to invoke graph evaluation
- surfacing recommendations in the dashboard
- reusing findings in compliance scoring and reporting
- using graph relationships as inputs to project and pipeline generation
