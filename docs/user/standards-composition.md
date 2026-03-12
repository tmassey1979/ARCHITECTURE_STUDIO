# Standards Composition

## What It Does

Architecture Studio is being built to assemble engineering standards automatically for a project context instead of forcing teams to piece guidance together manually.

The standards engine now has a seeded library covering:

- principles
- architecture
- frontend
- backend
- devops
- testing
- security
- observability
- process

## What Gets Considered

The composition engine can use:

- explicit project selections
- detected repository technologies
- detected repository tags and categories

That allows the extension to move toward standards guidance that is tailored instead of generic.

## Why This Matters

The composed standards result is intended to feed:

- dashboard summaries
- project generation
- report generation
- AI instructions

The result also preserves source metadata and selection reasons so teams can understand why a standard was included.

## Current Status

The `Architecture Studio: Compose Standards` command now resolves the active workspace and runs the deterministic standards-composition path against inferred project selections plus detected repository characteristics.

Richer UI integration is still planned, but the command path is now backed by the real engine instead of placeholder output.
