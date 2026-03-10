# Local Development

## Toolchain

- Node.js 22+ recommended
- npm 11+ recommended
- .NET SDK 9.0.311 currently pinned via `global.json`

## Why .NET 9 Right Now

The project rule is to prefer C# and move toward modern .NET for core engines. The current bootstrap is pinned to the installed SDK version available in this workspace today: `9.0.311`.

When the build environment is upgraded to .NET 10, the core projects can move forward in a controlled story rather than silently drifting per machine.

## Initial Setup

1. Install Node.js and the .NET SDK version from `global.json`.
2. From the repository root, run `npm install`.
3. Restore .NET dependencies with `dotnet restore core/ArchitectureStudio.sln`.

## Development Commands

- `npm run lint`
- `npm test`
- `npm run build`
- `npm run package:extension`
- `dotnet test core/ArchitectureStudio.sln`

## Recommended Validation Loop

1. Add or update a failing test first.
2. Implement the smallest change to get back to green.
3. Run:
   - `npm run verify`
   - `dotnet test core/ArchitectureStudio.sln`

## VS Code Debugging

This repository includes starter `.vscode` launch and task files for extension development.

Recommended flow:

1. Run the `npm: build` task.
2. Press `F5` in VS Code using the `Run Architecture Studio Extension` launch profile.
3. Use the Extension Development Host to invoke scaffolded commands from the command palette.

## Packaging

Run `npm run package:extension` to produce a `.vsix` package once dependencies are installed and the TypeScript build is green.
