# Issue 2 - Shared Contracts

## Summary

Established the first reusable shared-contract layer for Architecture Studio so the extension shell, UI boundary, and future C# engines can exchange a consistent schema.

## Delivered

- Added canonical C# contracts under `core/ArchitectureStudio.Core/Contracts/` for:
  - standards
  - regulations
  - controls
  - graph nodes and edges
  - findings and remediations
  - report artifacts
  - generated artifacts
  - project-selection profile
- Added canonical enums for severity, risk, graph relationships, categories, and artifact kinds.
- Added validation helpers:
  - `ContractValidation.ValidateProjectSelection`
  - `ContractValidation.ValidatePayload`
- Added JSON boundary helpers through `ContractJson`.
- Added TypeScript boundary mirror types in `src/contracts/sharedContracts.ts`.
- Added developer documentation for contract ownership and dependency direction in `docs/developer/shared-contracts.md`.
- Added failing-first tests and brought them to green for:
  - severity/risk levels
  - project-selection validation
  - JSON round-trip support

## Validation

- `dotnet test core/ArchitectureStudio.sln`
- `npm run verify`
- `npm run package:extension`
