# Compliance Library

## Purpose

Issue `#9` expands the compliance dataset from a small seed bundle into a structured regulation and control library that the compliance engine can consume directly.

The story delivers:

- regulation modules for the required baseline and expanded frameworks
- a broader technical control taxonomy
- schema validation through the catalog loader
- representative evaluation-path tests for each regulation family

## Layout

Controls:

- `compliance/controls/library.json`

Regulations:

- `compliance/regulations/gdpr.json`
- `compliance/regulations/ccpa.json`
- `compliance/regulations/coppa.json`
- `compliance/regulations/hipaa.json`
- `compliance/regulations/hitech.json`
- `compliance/regulations/sox.json`
- `compliance/regulations/pci-dss.json`
- `compliance/regulations/iso-27001.json`
- `compliance/regulations/nist-csf.json`
- `compliance/regulations/soc2.json`
- `compliance/regulations/tcpa.json`
- `compliance/regulations/can-spam.json`
- `compliance/regulations/pipeda.json`

## Regulation Schema

Each regulation module uses a consistent data contract with:

- `id`
- `title`
- `category`
- `jurisdiction`
- `summary`
- `required_controls`
- `data_types`
- `applicability`

The required schema fields in the story are enforced by `ComplianceCatalog.CreateDefault()` when it validates the loaded dataset.

## Control Taxonomy

The control library now includes the named controls required by the story, including:

- encryption
- audit logging
- role-based access control
- secrets management
- network segmentation
- data retention
- consent management

The library also expands into a broader taxonomy with controls such as:

- monitoring and alerting
- key management
- vulnerability management
- backup and recovery
- vendor risk review
- data loss prevention

## Engine Consumption

The compliance engine continues to consume the dataset without custom per-framework adapters. The catalog loader reads JSON modules directly, validates required fields, validates control references, and exposes typed definitions to the engine.

## Testing

Issue `#9` is covered by:

- `core/ArchitectureStudio.Core.Tests/ComplianceLibraryTests.cs`
  - regulation coverage
  - schema completeness
  - broader control coverage
  - representative evaluation path per regulation family
- `test/compliance/complianceLibraryArtifacts.test.ts`
  - module and documentation artifacts

## Extensibility

The current layout is intentionally compatible with later plugin-system work. Additional compliance packs can build on the same module schema instead of requiring engine code changes.
