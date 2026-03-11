# Report Export

## What It Does

Architecture Studio can now generate shareable reports and supporting documentation files from the analysis and compliance data it has collected.

## Current Export Formats

The current reporting engine supports:

- Markdown
- JSON
- SARIF

## Named Documentation Outputs

The generator also produces the named documentation files required by the product scope:

- `engineering-playbook.md`
- `security-policy.md`
- `incident-response.md`
- `architecture.md`

## Export Location

The current documented export path is `reports/`.

## PDF Status

PDF output is not implemented yet. Instead of failing report generation, the engine writes a documented fallback file under `reports/pdf-fallback.md` so the export flow stays usable.

## Why It Matters

This makes it possible to share findings and compliance posture outside the extension without requiring every stakeholder to open VS Code.
