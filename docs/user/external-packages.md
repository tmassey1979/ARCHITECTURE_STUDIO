# External Packages

Architecture Studio can be extended with external data packs that add organization-specific standards, compliance datasets, graph nodes, and generation templates.

## What External Packages Are For

Use external packages when you need to:

- add company or industry standards without editing the built-in seed package
- ship additional regulations and controls for a specific domain
- add graph data for cloud platforms, messaging stacks, or platform standards
- distribute reusable templates for projects, infrastructure, or documentation

## Where Packages Live

External packages are stored under:

- `plugins/packs/<package-id>/`

Each package includes a manifest named:

- `architecture-studio.package.json`

## What Users Will See

When packages are discovered, the dashboard `Standards` section shows:

- how many external packs were found
- whether each one loaded successfully
- the contribution kinds supplied by each pack
- any validation error for packages that failed to load

This makes it easy to tell whether a newly added package is ready before you rely on it in standards composition or generation flows.

## Sample Packs In This Repository

The repository currently includes:

- `aws-architecture-pack`
- `kafka-event-driven-pack`
- `banking-compliance-pack`

These examples show how to structure:

- cloud-specific standards and graph data
- event-driven architecture graph and template data
- domain compliance controls and regulations

## Package Installation Workflow

1. Add the package folder under `plugins/packs/`.
2. Confirm the package includes `architecture-studio.package.json`.
3. Verify every manifest path points to a real file inside that package folder.
4. Open the dashboard and check the `External Package Status` panel.
5. Fix any invalid-package message before depending on the new pack.

## Authoring Guidance

If you are creating a package rather than just installing one, use the developer guide:

- [Developer External Packages](../developer/external-packages.md)

## Screenshots

Screenshots for package status should be added under `docs/assets/external-packages/` as the dashboard workflow becomes more interactive.
