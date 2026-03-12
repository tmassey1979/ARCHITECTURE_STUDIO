# Report Export

## What It Does

Architecture Studio can now generate shareable reports and supporting documentation files from the analysis and compliance data it has collected.

## Current Export Formats

The current reporting engine supports:

- Markdown
- PDF
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

Architecture Studio now generates a real PDF artifact at:

- `reports/architecture-report.pdf`

The PDF is built from the same architecture, compliance summary, finding, evidence, and remediation content used in the other export formats.

## Why It Matters

This makes it possible to share findings and compliance posture outside the extension without requiring every stakeholder to open VS Code.
