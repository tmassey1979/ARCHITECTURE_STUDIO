# Project Generation

## What It Does

Architecture Studio can now compose starter project artifacts from a selected solution profile instead of forcing every generated project through one fixed scaffold.

The current generator accepts the required selection areas:

- frontend
- backend
- architecture pattern
- CI/CD
- infrastructure
- compliance targets

## What Gets Generated

The current template set can produce starter artifacts across the required structure areas, including:

- `src`
- `services`
- `frontend`
- `infrastructure`
- `docs`
- `docker`
- `k8s`

It also supports pipeline output for:

- GitHub Actions
- GitLab CI
- Jenkins
- Azure DevOps
- CircleCI

## Why It Matters

This gives the plugin a real generation foundation for turning architecture selections into working starting points. It also keeps the template system modular so new stacks and compliance overlays can be added without replacing the whole generator.

## Current Dataset Direction

The initial template library already covers a mix of:

- frontend variations
- backend variations
- architecture-pattern variations
- compliance overlays
- devops variations

That aligns the generator with the project-template dataset direction in `codex/data.md`.

## Documentation Notes

This story focuses on the template catalog and composition engine. Richer interactive selection UX and screenshots will come once the dashboard and command flows expose the full generator experience.
