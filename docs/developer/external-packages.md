# External Packages

## Purpose

Architecture Studio external packages extend the built-in standards, compliance, graph, and generation datasets without moving core engine logic into the extension host.

This story keeps the execution model data-only:

- package discovery is handled in C#
- package contracts are JSON and YAML files on disk
- package loading validates structure and file references
- invalid packages fail gracefully and do not block valid packages

## Package Layout

Each package lives under `plugins/packs/<package-id>/` and must contain:

- `architecture-studio.package.json`
- one or more contribution files referenced by the manifest

Current sample packs:

- `plugins/packs/aws-architecture-pack/`
- `plugins/packs/kafka-event-driven-pack/`
- `plugins/packs/banking-compliance-pack/`

## Manifest Contract

The TypeScript mirror contract lives in [`src/plugins/externalPackages.ts`](../../src/plugins/externalPackages.ts).

The C# loader contract lives in:

- `core/ArchitectureStudio.Core/Plugins/ExternalPackageModels.cs`
- `core/ArchitectureStudio.Core/Plugins/ExternalPackageLoader.cs`

Manifest shape:

```json
{
  "id": "aws-architecture-pack",
  "version": "1.0.0",
  "schemaVersion": "1.0.0",
  "displayName": "AWS Architecture Pack",
  "contributions": {
    "standardsPackages": ["standards/aws-standards.json"],
    "regulations": [],
    "controls": [],
    "templates": ["templates/aws-reference-stack.json"],
    "graphDatasets": ["graph/aws-services.yml"]
  }
}
```

Supported contribution points:

- `standardsPackages`
- `regulations`
- `controls`
- `templates`
- `graphDatasets`

## Validation Rules

The loader enforces the following rules before a package is treated as loaded:

- manifest metadata must include `id`, `version`, `schemaVersion`, and `displayName`
- contribution file paths must be package-relative
- contribution file paths cannot escape the package root
- every referenced contribution file must exist
- standards packages must deserialize into the standards package schema
- compliance datasets must deserialize into valid control or regulation entries
- templates must deserialize into valid template definitions with at least one file
- graph datasets must deserialize into YAML documents with at least one valid node
- duplicate package ids are rejected

When a package fails validation:

- it is reported with status `Invalid`
- the failure message is preserved
- the rest of discovery continues

## Runtime Surface

Package discovery is now consumed by the default C# runtime, not just surfaced in the dashboard. Valid packs are merged into the active catalogs used by:

- `StandardsCatalog.CreateDefault()`
- `TechnologyGraphCatalog.CreateDefault()`
- `ComplianceCatalog.CreateDefault()`
- `ProjectTemplateCatalog.CreateDefault()`
- `StudioWorkspaceOrchestrator.CreateDefault()`

The runtime merge path is implemented in:

- `core/ArchitectureStudio.Core/Runtime/StudioRuntimeCatalogFactory.cs`

That means external package content now influences:

- standards composition
- architecture graph evaluation and recommendations
- compliance regulation/control matching
- project and infrastructure generation

The dashboard standards section still surfaces package load status in a user-visible panel. That keeps discovery visible without requiring a debugger or hidden logs.

Status contract:

- `Loaded`
- `Invalid`

Contribution kind summaries:

- `Standards`
- `Compliance`
- `Templates`
- `Graph`

## Test Coverage

The story is driven by:

- `core/ArchitectureStudio.Core.Tests/ExternalPackageLoaderTests.cs`
- `core/ArchitectureStudio.Core.Tests/ExternalPackageRuntimeIntegrationTests.cs`
- `test/plugins/externalPackageArtifacts.test.ts`
- `test/dashboard/dashboardState.test.ts`

These tests cover:

- discovery of required sample packs
- contribution point validation
- graceful handling for invalid packages
- runtime application of standards, graph, compliance, and template contributions
- dashboard projection of package status
- presence of docs and manifest artifacts
