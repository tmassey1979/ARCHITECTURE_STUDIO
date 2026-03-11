# Standards

This folder contains standards-library content, schemas, and composition assets.

## Current Layout

- `packages/architecture-studio.seed.json`
  - the built-in standards package consumed by the C# standards composition engine

## Package Shape

Standards packages are JSON documents containing:

- package metadata
  - `id`
  - `version`
  - `sourcePath`
- standards entries
  - `definition`
  - `appliesToCategories`
  - `appliesToSelections`
  - `appliesToTags`
  - `sourceTitle`

## Extension Point

The core engine is designed so future external standards packages can be loaded and merged into the catalog without rewriting the composition engine itself.
