# Architecture Reasoning

## What This Feature Does

Architecture Studio now includes a curated technology graph and a first-pass architecture reasoning engine. This feature is the basis for future guided architecture generation, compatibility checks, and validation workflows inside the extension.

At this stage, the engine can:

- understand technologies, frameworks, architecture patterns, regulations, and controls
- model required, conflicting, paired, and recommended relationships
- evaluate a selected stack for compatibility gaps
- flag a core set of architecture boundary violations

## Why It Matters

This gives the plugin a structured way to answer questions such as:

- What technologies are missing from the current stack selection?
- Which choices conflict with each other?
- Which additional components are usually recommended?
- Are core architecture boundaries being broken?

That reasoning will later power guided workflows in the dashboard, report generation, compliance scoring, and project scaffolding.

## Included Coverage

The shipped graph content already includes broad starting coverage for:

- frontend frameworks
- backend frameworks
- cloud platforms
- databases
- CI/CD tooling
- messaging infrastructure
- observability tooling
- security tooling
- architecture patterns
- governance and regulation anchors

The initial architecture patterns include:

- Clean Architecture
- Hexagonal Architecture
- Onion Architecture
- Layered Architecture
- Vertical Slice Architecture
- Microservices
- Event Driven Architecture

## Current Validation Rules

The first architecture validation pass detects:

- domain code directly referencing infrastructure
- business logic placed in UI code
- controllers directly accessing the database
- missing authentication configuration

When these conditions are found, the engine produces findings with severity, risk, evidence, and remediation guidance.

## What To Expect In The Extension

The `Architecture Studio: Generate Architecture` command now evaluates the active workspace through the shipped graph and validation rules. Richer UX is still planned, including:

- dashboard views for recommendations and conflicts
- stack validation commands
- richer generated reports
- deeper integration with compliance and generation workflows

## Documentation Notes

Screenshots are not included for this story because the engine is delivered ahead of the dedicated reasoning UI. Once the reasoning workflow is exposed in the dashboard, screenshots will be added under `docs/assets/`.
