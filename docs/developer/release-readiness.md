# Release Readiness

## Purpose

This guide is the maintainer-facing checklist for getting Architecture Studio from a green local workspace to a releasable VS Code extension package.

Use it alongside:

- [Local Development](./local-development.md)
- [Release And Publishing](./release-and-publishing.md)

## Required Setup

1. Install the Node.js and .NET versions documented in [Local Development](./local-development.md).
2. Run `npm install` from the repository root.
3. Restore .NET packages with `dotnet restore core/ArchitectureStudio.sln`.

## Standard Validation Commands

These are the commands contributors are expected to use locally, and they are the same commands run in GitHub Actions:

- `dotnet test core/ArchitectureStudio.sln`
- `npm run verify`
- `npm run package:extension`

## Debug Workflow

Recommended day-to-day debug loop:

1. Add or update a failing automated test first.
2. Run the smallest relevant test loop until green.
3. Run `npm run verify`.
4. Run `dotnet test core/ArchitectureStudio.sln`.
5. Press `F5` in VS Code with the `Run Architecture Studio Extension` launch profile.
6. Use the Extension Development Host to exercise commands and the dashboard.
7. Inspect the `Architecture Studio` output channel when a command fails.

## Smoke Fixture Workflow

The curated smoke workspace lives under:

- `fixtures/sample-workspaces/fintech-platform/`

It is used to validate an end-to-end path across:

- repository analysis
- compliance evaluation
- report generation
- PDF report export

The smoke test is implemented in:

- `core/ArchitectureStudio.Core.Tests/SmokeFixtureTests.cs`

Use that fixture when you need a stable, reviewable workspace that triggers:

- ASP.NET Core detection
- GitHub Actions detection
- clean architecture detection
- personal-data classification
- financial-data classification
- PCI DSS and related compliance scoring

The fixture is repository-only validation data and is excluded from the shipped VSIX package.

## Packaging Workflow

When the repo is green:

1. Run `npm run package:extension`.
2. Confirm the published core host exists under `core-host/`.
3. Confirm the `.vsix` file is produced at the repository root.
4. Validate that the package includes the compiled extension, the packaged C# host, datasets, docs-linked assets, and plugin sample packs.
5. Attach or upload the generated `.vsix` through the release workflow when preparing a public release.

## CI Expectations

The CI and release workflows should both:

- install Node dependencies
- install the pinned .NET SDK from `global.json`
- run `dotnet test core/ArchitectureStudio.sln`
- run `npm run verify`
- run `npm run package:extension`

That keeps local and CI validation aligned.

## Known Limitations

- Packaged command execution depends on `dotnet` being available so the bundled `ArchitectureStudio.Cli.dll` can run.
- Marketplace publishing still requires a valid `VSCE_PAT` secret and a ready publisher configuration.
- GitHub Pages deployment still depends on repository settings enabling `GitHub Actions` as the Pages source.
- Screenshot coverage in the docs is still incomplete and should expand as more command flows become interactive.
