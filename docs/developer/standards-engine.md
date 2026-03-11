# Standards Engine

## Purpose

The standards engine provides a deterministic, explainable standards-composition layer for Architecture Studio. It is intended to supply standards guidance to:

- the dashboard
- project generation
- report generation
- AI instruction generation

## Core Design

The implementation lives primarily in the C# core library:

- `core/ArchitectureStudio.Core/Standards/StandardsModels.cs`
- `core/ArchitectureStudio.Core/Standards/StandardsCatalog.cs`
- `core/ArchitectureStudio.Core/Standards/StandardsCompositionEngine.cs`
- `core/ArchitectureStudio.Core/Standards/StandardsJson.cs`

The seed package lives in:

- `standards/packages/architecture-studio.seed.json`

## Composition Inputs

The engine composes a result from two signal sources:

- explicit project selections
  - frontend
  - backend
  - architecture pattern
  - CI/CD tools
  - infrastructure
  - additional selections
- detected repository characteristics
  - technologies
  - tags
  - categories

## Determinism

Output ordering is stable:

- by standard category
- then by title
- then by id

Selection reasons are normalized and deduplicated so repeated runs over the same input produce the same result.

## Explainability

Each composed standard carries:

- the shared-contract `StandardDefinition`
- source metadata
  - package id
  - package version
  - source path
  - source title
- selection reasons

That makes downstream reports and AI prompts auditable instead of opaque.

## Extension Points

`StandardsCatalog.WithPackage(...)` allows additional packages to be merged into the catalog without changing engine logic. That is the intended path for future plugin-provided standards packs.

## TypeScript Boundary

TypeScript remains transport-only for this story. The mirror types live in:

- `src/standards/standardsComposition.ts`

These types are intended for dashboard, generator, report, and AI instruction consumers after the C# engine result is surfaced into the extension host.
