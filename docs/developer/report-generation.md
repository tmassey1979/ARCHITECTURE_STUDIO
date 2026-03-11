# Report Generation

## Purpose

Issue `#11` adds the first reporting engine for Architecture Studio. The engine generates shareable exports and supporting documentation artifacts from findings and compliance summaries.

The story delivers:

- Markdown, JSON, and SARIF report generation
- named supporting documentation files
- deterministic export paths under `reports/`
- an explicit PDF fallback that does not break generation

## Engine Files

- `core/ArchitectureStudio.Core/Reporting/ReportGenerationModels.cs`
- `core/ArchitectureStudio.Core/Reporting/ReportGenerationEngine.cs`

## Outputs

Current generated files include:

- `reports/architecture-report.md`
- `reports/compliance-report.json`
- `reports/findings.sarif`
- `reports/engineering-playbook.md`
- `reports/security-policy.md`
- `reports/incident-response.md`
- `reports/architecture.md`
- `reports/pdf-fallback.md`

## Format Notes

- Markdown provides human-readable architecture and operational summaries.
- JSON preserves stable schema data for automation and downstream tooling.
- SARIF maps findings into a code-scanning-friendly format with severity and remediation fields.
- PDF is not implemented in this story; the engine emits `reports/pdf-fallback.md` as the documented fallback path.

## TypeScript Boundary

TypeScript owns:

- report transport types in `src/reports/reportGeneration.ts`
- the `Generate Reports` command handler

The command resolves the current workspace and consumes the report-generation result through a service boundary.

## Testing

Issue `#11` is covered by:

- `core/ArchitectureStudio.Core.Tests/ReportGenerationEngineTests.cs`
  - export formats
  - named docs
  - PDF fallback
  - deterministic output
- `test/commands/generateReportsHandler.test.ts`
  - workspace targeting
  - output reporting
  - no-workspace handling
- `test/reports/reportArtifacts.test.ts`
  - transport and documentation artifacts
