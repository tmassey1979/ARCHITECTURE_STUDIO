# Compliance Validation

## What It Does

Architecture Studio can now determine which regulations are likely relevant, score the current level of control coverage, and show the remediation gaps that still need work.

The first version focuses on:

- determining applicable regulations
- scoring control coverage
- showing missing controls
- exposing remediation guidance
- rendering summary cards such as `HIPAA 72%`

## How Applicability Is Determined

The engine uses a combination of:

- system characteristics
- detected repository technologies
- classified sensitive-data categories

This means the plugin can ground compliance posture in real repository evidence instead of generic checklists.

## Current Seeded Coverage

The initial compliance catalog includes seed coverage for examples such as:

- HIPAA
- GDPR
- PCI DSS
- COPPA
- SOC 2

This catalog will expand in later stories, but the engine and score format are already in place.

## What The Score Means

Each applicable regulation gets a percentage score based on how many required controls are currently covered.

Examples:

- `HIPAA 72%`
- `SOC 2 100%`

The score is intended as an explainable working posture, not a legal certification result.

## Findings And Remediation

When required controls are missing, the plugin produces findings that include:

- severity
- risk
- evidence
- remediation guidance

This gives teams a concrete remediation path instead of just a failing status.

## Command Behavior

The `Validate Regulations` command now targets the active VS Code workspace automatically. You do not need to manually provide the current workspace path.

## Dashboard Behavior

The dashboard can now render compliance summary cards in the expected style, alongside the detailed findings and remediation focus panels.

## Documentation Notes

Screenshots are still pending because this story adds the scoring engine and summary rendering before the richer interactive compliance drill-down UI. Screenshots will be added once the downstream compliance dashboard flows land.
