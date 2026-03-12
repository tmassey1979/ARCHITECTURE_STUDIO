# Report Generation

## Purpose

Issue `#11` adds the first reporting engine for Architecture Studio. The engine generates shareable exports and supporting documentation artifacts from findings and compliance summaries.

The story delivers:

- Markdown, JSON, and SARIF report generation
- deterministic PDF report generation
- named supporting documentation files
- deterministic export paths under `reports/`

## Engine Files

- `core/ArchitectureStudio.Core/Reporting/ReportGenerationModels.cs`
- `core/ArchitectureStudio.Core/Reporting/ReportGenerationEngine.cs`

## Outputs

Current generated files include:

- `reports/architecture-report.md`
- `reports/architecture-report.pdf`
- `reports/compliance-report.json`
- `reports/findings.sarif`
- `reports/engineering-playbook.md`
- `reports/security-policy.md`
- `reports/incident-response.md`
- `reports/architecture.md`

## Format Notes

- Markdown provides human-readable architecture and operational summaries.
- PDF provides a portable document artifact built from the same architecture, compliance, finding, evidence, and remediation content.
- JSON preserves stable schema data for automation and downstream tooling.
- SARIF maps findings into a code-scanning-friendly format with severity and remediation fields.
- The PDF renderer is implemented directly in C# and does not rely on external services or native PDF tooling.
- The current PDF layout is intentionally text-first and deterministic; it favors portability and CI safety over advanced styling.

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
  - PDF artifact generation
  - deterministic output
- `test/commands/generateReportsHandler.test.ts`
  - workspace targeting
  - output reporting including PDF export status
  - no-workspace handling
- `test/reports/reportArtifacts.test.ts`
  - transport and documentation artifacts
