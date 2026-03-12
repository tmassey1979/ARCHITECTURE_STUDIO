# Automation CLI

Architecture Studio includes a .NET CLI host for automation scenarios where you want the engine outputs without opening VS Code.

## When To Use It

Use the CLI when you want to:

- run repository analysis in CI
- generate reports from a script
- build AI-instruction payloads for another tool
- drive project generation from automation

## Packaged Host Path

After packaging the extension, the CLI host lives under:

- `core-host/ArchitectureStudio.Cli.dll`

Use `dotnet` to invoke it:

```powershell
dotnet core-host/ArchitectureStudio.Cli.dll --help
```

## Workspace Commands

Workspace-driven commands accept `--workspace <path>`.

Example:

```powershell
dotnet core-host/ArchitectureStudio.Cli.dll analyze-repository --workspace fixtures/sample-workspaces/fintech-platform
```

These commands return JSON to standard output:

- `analyze-repository`
- `compose-standards`
- `evaluate-architecture`
- `validate-regulations`
- `infer-project-selection`
- `generate-reports`
- `build-ai-instruction-request`

## Standard-Input Commands

Some commands accept JSON on standard input instead of a workspace switch.

Examples:

```powershell
Get-Content project-selection.json | dotnet core-host/ArchitectureStudio.Cli.dll generate-project
```

```powershell
Get-Content ai-request.json | dotnet core-host/ArchitectureStudio.Cli.dll generate-ai-instructions
```

## Automation Behavior

- successful commands return exit code `0`
- invalid usage returns a non-zero exit code
- command output is JSON on standard output
- validation and parser errors are written to standard error
- standard-input commands accept normal UTF-8 JSON payloads, including PowerShell-style BOM-prefixed input

## Related Docs

- [Repository Analysis](./repository-analysis.md)
- [Report Export](./report-export.md)
- [AI Instructions](./ai-instructions.md)
- [Developer Core CLI Bridge](../developer/core-cli-bridge.md)
