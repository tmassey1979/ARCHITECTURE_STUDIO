# Compliance Engine

## Purpose

Issue `#8` adds the first explainable compliance engine for Architecture Studio. The engine determines applicable regulations, scores control coverage, and emits typed remediation findings that downstream reporting and dashboard features can consume.

The story delivers:

- a seed compliance catalog under `compliance/`
- a C# compliance catalog loader and scoring engine
- shared compliance summary contracts
- a workspace-aware `Validate Regulations` command boundary
- dashboard support for score cards such as `HIPAA 72%`

## Seed Catalog

The current seed catalog lives in:

- `compliance/controls/seed-controls.json`
- `compliance/regulations/seed-regulations.json`

The catalog is intentionally data-driven so the later regulation-library story can expand coverage without rewriting the engine.

Current seeded frameworks and regulations include examples such as:

- HIPAA
- GDPR
- PCI DSS
- COPPA
- SOC 2

Each regulation carries:

- required controls
- applicability rules based on system characteristics
- applicability rules based on detected technologies
- applicability rules based on classified sensitive-data categories

## Engine Flow

The engine follows the required four-step validator flow from `codex/studio.md`:

1. detect characteristics from repository-analysis output and request inputs
2. determine which regulations apply
3. evaluate required controls against implemented controls
4. generate score summaries and remediation findings

Core files:

- `core/ArchitectureStudio.Core/Compliance/ComplianceModels.cs`
- `core/ArchitectureStudio.Core/Compliance/ComplianceCatalog.cs`
- `core/ArchitectureStudio.Core/Compliance/ComplianceEngine.cs`

## Scoring

For each applicable regulation, the engine calculates:

- covered controls
- total required controls
- integer score percentage

The output is deterministic and sorted so the same repository inputs produce the same result ordering and serialized output.

## Findings

Missing controls produce typed `FindingDefinition` entries with:

- severity
- risk
- remediation title
- remediation summary
- evidence describing why the regulation applied and the current score

This keeps the engine explainable enough for later report-generation stories.

## Shared Contracts

Compliance summaries are now part of the shared payload shape through `ComplianceSummary`. That lets the dashboard and future report/export features render score cards without inventing parallel UI-only schemas.

## TypeScript Boundary

The TypeScript side owns:

- transport models in `src/compliance/complianceEvaluation.ts`
- the validate command handler
- dashboard rendering of compliance score cards

The `Validate Regulations` command resolves the current workspace automatically and consumes compliance results through a service boundary, keeping TypeScript thin and ready for a later C# process bridge.

## Testing

Issue `#8` is covered by:

- `core/ArchitectureStudio.Core.Tests/ComplianceEngineTests.cs`
  - applicable regulation detection
  - score calculation
  - missing-control findings
  - repeatable output
- `test/commands/validateRegulationsHandler.test.ts`
  - workspace targeting
  - summary output
  - no-workspace behavior
- `test/dashboard/dashboardState.test.ts`
  - compliance score card rendering
- `test/compliance/complianceArtifacts.test.ts`
  - transport and documentation artifacts

## Follow-On Work

Later stories should extend this by:

- expanding the regulation and control library depth
- wiring real repository-analysis output into command execution
- surfacing richer compliance drill-down views in the dashboard
- exporting detailed compliance reports
