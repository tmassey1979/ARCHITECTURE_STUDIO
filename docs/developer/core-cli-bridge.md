# Core CLI Bridge

## Purpose

Architecture Studio keeps the VS Code shell thin by routing command execution into a C# host process.

That bridge now lives at:

- `src/core/architectureStudioCoreCli.ts`
- `core/ArchitectureStudio.Cli/`
- `core/ArchitectureStudio.Core/Workspace/StudioWorkspaceOrchestrator.cs`

## Runtime Model

The extension uses two launch modes:

- packaged mode: `dotnet core-host/ArchitectureStudio.Cli.dll`
- local development mode: `dotnet run --project core/ArchitectureStudio.Cli --no-build --`

The Node side chooses packaged mode first and falls back to the source-project mode when the published host is not present.

The CLI itself now uses `System.CommandLine`, which gives the published host structured help output, required-option validation, and stable command routing for automation.

## Why This Exists

This keeps:

- command registration in TypeScript
- domain logic in C#
- transport JSON-based and auditable

It also avoids duplicating engine logic across TypeScript and C#.

## Exposed Command Surface

The CLI currently exposes:

- `analyze-repository`
- `compose-standards`
- `evaluate-architecture`
- `validate-regulations`
- `infer-project-selection`
- `generate-project`
- `generate-reports`
- `build-ai-instruction-request`
- `generate-ai-instructions`

Workspace-driven commands accept `--workspace <path>`.

Request-driven commands read JSON from standard input.

## Command-Line Examples

Show help:

```powershell
dotnet core-host/ArchitectureStudio.Cli.dll --help
```

Run a workspace command:

```powershell
dotnet core-host/ArchitectureStudio.Cli.dll validate-regulations --workspace fixtures/sample-workspaces/fintech-platform
```

Run a standard-input command:

```powershell
Get-Content ai-request.json | dotnet core-host/ArchitectureStudio.Cli.dll generate-ai-instructions
```

Automation contract:

- success returns exit code `0`
- parse or validation failures return a non-zero exit code
- successful command results are written as JSON to standard output
- parser and execution errors are written to standard error
- standard-input commands normalize UTF-8 BOM-prefixed JSON so common shell pipelines keep working

## Workspace Orchestration

`StudioWorkspaceOrchestrator` is responsible for:

- repository analysis
- project-selection inference
- standards composition inputs
- architecture graph evaluation
- compliance-request inference
- report-generation inputs
- AI-instruction request generation

This keeps the command-line entry point thin and the orchestration testable from the core test suite.

## Packaging Notes

The VSIX includes the published host under `core-host/`.

Repository source under `core/` is still excluded from the package. The shipped extension uses the published host, not the source tree.
