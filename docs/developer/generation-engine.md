# Generation Engine

## Purpose

Issue `#10` adds the first modular generation engine for Architecture Studio. The engine composes project, pipeline, infrastructure, and documentation starter artifacts from reusable template fragments instead of one monolithic generator.

The story delivers:

- a C# template catalog and composition engine
- modular JSON templates under `templates/`
- a thin TypeScript transport model for project generation
- a selection-aware `Generate Project` command boundary

## Template Layout

Project templates:

- `templates/projects/foundation/`
- `templates/projects/frontend/`
- `templates/projects/backend/`
- `templates/projects/architecture/`
- `templates/projects/compliance/`

Pipeline templates:

- `templates/pipelines/`

Infrastructure templates:

- `templates/infra/`

The dataset is intentionally split by reusable concern so future plugin packs can add fragments without rewriting the generator flow.

## Covered Variations

The initial dataset covers the required story scope and the multi-template direction from `codex/data.md`, including examples such as:

- React, Angular, React Native, WPF, and Blazor frontends
- ASP.NET Core, Spring Boot, and FastAPI backends
- Clean Architecture, Hexagonal Architecture, Microservices, CQRS, and Event Sourcing patterns
- HIPAA, GDPR, PCI DSS, and SOC 2 documentation overlays
- GitHub Actions, GitLab CI, Jenkins, Azure DevOps, and CircleCI pipelines
- Docker, Kubernetes, Helm, Terraform, and Bicep infrastructure variations

## Engine Flow

Core files:

- `core/ArchitectureStudio.Core/Generation/ProjectGenerationModels.cs`
- `core/ArchitectureStudio.Core/Generation/ProjectTemplateCatalog.cs`
- `core/ArchitectureStudio.Core/Generation/ProjectGenerationEngine.cs`

The generator:

1. loads the template fragments from `templates/`
2. matches fragments against the selected frontend, backend, architecture pattern, CI/CD, infrastructure, and compliance targets
3. renders tokenized file paths and contents deterministically
4. returns generated files plus generated-artifact metadata

## Determinism

For the same selection set, the engine returns:

- the same template IDs
- the same generated file ordering
- the same generated artifact ordering

That makes it safe for tests and later report/export features.

## TypeScript Boundary

TypeScript owns:

- transport types in `src/generators/projectGeneration.ts`
- the `Generate Project` command handler
- future orchestration from UI-driven selection workflows

The command currently consumes a selection through a service boundary and reports the generated artifact count plus the template set used.

## Testing

Issue `#10` is covered by:

- `core/ArchitectureStudio.Core.Tests/ProjectGenerationEngineTests.cs`
  - template coverage
  - deterministic generation
  - required output structure
- `test/commands/generateProjectHandler.test.ts`
  - selection-aware command behavior
  - missing-selection error handling
- `test/generators/generatorArtifacts.test.ts`
  - transport, docs, and template artifact coverage

## Follow-On Work

Later stories should extend this by:

- writing the generated files into a selected workspace location
- surfacing selection flows in the dashboard
- adding richer architecture and compliance documentation stubs
- emitting AGENTS.md and report artifacts directly from the same composed template set
