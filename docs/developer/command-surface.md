# Command Surface

This document defines the stable command entry points exposed by the VS Code extension host.

## Output Channel

- Name: `Architecture Studio`
- Purpose: shared logging surface for command routing, placeholder execution, and future engine diagnostics

## Command Routes

| Command ID | Title | Handler Module | Current Behavior |
| --- | --- | --- | --- |
| `architectureStudio.openDashboard` | `Architecture Studio: Open Dashboard` | `./handlers/openDashboardHandler` | Routes through the centralized command runtime and shows scaffolded placeholder output |
| `architectureStudio.composeStandards` | `Architecture Studio: Compose Standards` | `./handlers/composeStandardsHandler` | Routes through the centralized command runtime and shows scaffolded placeholder output |
| `architectureStudio.analyzeRepository` | `Architecture Studio: Analyze Repository` | `./handlers/analyzeRepositoryHandler` | Resolves the active workspace, invokes the repository-analysis service boundary, and reports structured result counts |
| `architectureStudio.validateRegulations` | `Architecture Studio: Validate Regulations` | `./handlers/validateRegulationsHandler` | Resolves the active workspace, invokes the compliance service boundary, and reports regulation score summaries plus finding counts |
| `architectureStudio.generateArchitecture` | `Architecture Studio: Generate Architecture` | `./handlers/generateArchitectureHandler` | Routes through the centralized command runtime and shows scaffolded placeholder output |
| `architectureStudio.generateProject` | `Architecture Studio: Generate Project` | `./handlers/generateProjectHandler` | Consumes a selected project profile, invokes the generation service boundary, and reports generated artifact counts plus the applied template set |
| `architectureStudio.generateReports` | `Architecture Studio: Generate Reports` | `./handlers/generateReportsHandler` | Resolves the active workspace, invokes the report-generation service boundary, and reports export counts plus PDF fallback state |
| `architectureStudio.generateAiInstructions` | `Architecture Studio: Generate AI Instructions` | `./handlers/generateAiInstructionsHandler` | Resolves AI-instruction context, invokes the instruction-generation service boundary, and reports whether `AGENTS.md` plus related guidance were generated |

## Design Notes

- `src/commands/commandRuntime.ts` owns route discovery, lazy handler loading, and error reporting.
- `src/commands/registerCommands.ts` stays thin and only wires VS Code APIs to the runtime.
- Handler modules are separate so downstream stories can replace placeholders without changing activation flow.
