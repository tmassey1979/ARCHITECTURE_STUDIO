# Shared Contracts

The shared-contract layer is the canonical schema for Architecture Studio.

## Ownership Rules

- Canonical shared contracts live in the C# core library under `core/ArchitectureStudio.Core/Contracts/`.
- Core engines should depend on these contracts instead of inventing module-local DTOs.
- The TypeScript extension shell may use boundary mirror types from `src/contracts/sharedContracts.ts`.
- TypeScript mirror types are for transport and UI composition only. They do not replace the C# source of truth.
- Shared contracts must not depend on:
  - VS Code APIs
  - webview-specific UI models
  - engine-specific implementation services
  - filesystem or infrastructure adapters

## Dependency Direction

- Contracts are at the bottom of the dependency graph.
- Engine modules may reference contracts.
- Serialization, validation, and mapping helpers may reference contracts.
- Contracts must not reference standards, graph, compliance, generator, or report engine implementations.

## Boundary Rules

- JSON exchanged between C# engines and the TypeScript shell should use the shared contract payload shape.
- Enum values are serialized as strings.
- IDs must be stable and deterministic across standards, controls, regulations, findings, reports, and generated artifacts.
- UI layers should render directly from shared contracts or dedicated view models mapped from them without mutating the underlying schema.

## Validation Rules

- `ContractValidation.ValidateProjectSelection` validates required project-selection fields.
- `ContractValidation.ValidatePayload` validates duplicate IDs and graph-edge references.
- Consumers should validate shared payloads at system boundaries before using them.

## Current Contract Coverage

- standards
- regulations
- controls
- graph nodes and edges
- findings and remediations
- compliance summaries
- report artifacts
- generated artifacts
- project-selection profile
