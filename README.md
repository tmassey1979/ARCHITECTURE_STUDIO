# Architecture Studio

Architecture Studio is a VS Code extension workspace for architecture generation, repository analysis, compliance validation, standards composition, reporting, and AI-assisted delivery guidance.

## Current Status

This repository currently contains the bootstrap scaffold for the extension shell, C# core workspace, documentation site, and release automation.

## Quick Start

- Read the user and developer docs from [docs/index.md](docs/index.md)
- Follow local setup in [docs/developer/local-development.md](docs/developer/local-development.md)
- Run `npm install`
- Run `npm run verify`
- Run `dotnet test core/ArchitectureStudio.sln`

## Structure

- `src/` VS Code extension entrypoint and TypeScript shell
- `core/` C# solution for reusable business logic
- `analysis/`, `generators/`, `graph/`, `standards/`, `compliance/`, `templates/`, `reports/` product work areas
- `docs/` user and developer documentation plus GitHub Pages content
