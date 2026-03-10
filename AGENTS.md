# Architecture Studio Agent Guide

## Purpose

Build `Architecture Studio` as a modular architecture-intelligence platform delivered through a VS Code extension shell.

The extension must support:

- standards composition
- architecture reasoning
- repository analysis
- compliance validation
- project and pipeline generation
- report and documentation generation
- AI instruction generation

Primary requirement sources:

- `codex/studio.md`
- `codex/data.md`

## Delivery Rules

- Build with TDD. Start each implementation change by adding or updating a failing test.
- Keep the red, green, refactor loop explicit in issue comments and completion notes.
- Prefer C# where possible.
- Use `net10.0` for new .NET code unless the repository is explicitly pinned differently later.
- Keep TypeScript limited to the VS Code extension host, webview UI, and thin interop boundaries.
- Keep generated datasets, templates, and configuration assets in text formats that are easy to diff and validate.
- Treat documentation as a required deliverable for major features, not optional cleanup.

## Language And Architecture Split

- TypeScript owns:
  - VS Code activation
  - command registration
  - webview UI
  - extension-to-engine orchestration
- C# owns where practical:
  - standards composition logic
  - technology graph logic
  - architecture validation logic
  - repository analysis engines
  - compliance and scoring engines
  - report generation logic
  - project/template generation logic
- Do not move core business rules into TypeScript when a C# component or library can own them cleanly.
- Favor reusable C# libraries and test projects over one-off scripts.

## Testing Standard

- Every feature starts with a failing automated test.
- Prefer layered tests:
  - unit tests for pure domain logic
  - integration tests for extension orchestration, process boundaries, and generated outputs
  - smoke tests for end-to-end command flows
- A story is not complete until relevant tests pass locally.
- Fixing a defect requires a regression test first.

## Issue Rules

Use the same issue discipline as `DragonEnvelopes`.

### Issue Types

- `Feature: ...` for planned functionality
- `Bug: ...` for defects
- `Epic: ...` for cross-story delivery tracking

### Required Story Format

Each feature or bug issue should contain:

- `Story`
- `Acceptance Criteria`
- `Dev Notes`
- `Lifecycle Checklist`

Use story format:

`As <x>, I would like <y>, so that <z>.`

### Lifecycle Checklist

- [ ] Add `inprogress` label when implementation starts.
- [ ] Add start comment with scope and plan.
- [ ] Add or update a failing test before production code changes.
- [ ] Add progress comments during work.
- [ ] Validate with build/tests/smoke checks.
- [ ] Add changelog file under `changelog/`.
- [ ] Add completion comment with validation, commit hash, and time spent.
- [ ] Remove `inprogress`.
- [ ] Close issue.

### Execution Expectations

- Work from an issue unless the task is explicitly ad hoc repo setup.
- Keep acceptance criteria testable and objective.
- Call out dependencies in `Dev Notes`.
- Use issue comments to show TDD progress, not just final status.

## Coding Standards

- Follow Clean Code, SOLID, and modular architecture.
- Keep boundaries explicit between extension shell, UI, datasets, generators, and rule engines.
- Use deterministic outputs for reports, templates, and generated docs.
- Avoid hidden network dependencies in analysis or compliance logic.
- Keep severity and compliance scoring contracts consistent across the system.

## Compliance And Documentation

- Compliance logic must be explainable and auditable.
- Findings must carry severity, evidence, and remediation.
- Generated outputs should include documentation and AI instruction artifacts when applicable.
- Treat dataset scope in `codex/data.md` as required delivery content, not optional examples.

## Documentation Standard

- Maintain all project documentation in Markdown under `docs/`.
- Keep documentation organized into two primary sections:
  - `docs/user/` for end-user docs, onboarding, workflows, troubleshooting, and plugin usage
  - `docs/developer/` for architecture, setup, contribution guidance, testing, and internal implementation details
- Keep a landing page at `docs/index.md` that works both as a documentation home page and as a GitHub Pages entry point.
- Write detailed documentation by default. Favor complete walkthroughs, examples, screenshots, and operational notes over thin summaries.
- Add screenshots where practical for dashboard flows, command flows, generated outputs, and key UX states.
- Store screenshots and other documentation images under `docs/assets/` with clear names grouped by feature area.
- Keep user-facing documentation understandable by non-developers.
- Keep developer documentation precise enough that a new contributor can build, test, debug, and extend the system without reverse engineering the codebase.
- Update user and developer documentation as part of the same story when behavior, workflows, configuration, or architecture changes.
- Documentation should be GitHub Pages friendly:
  - use relative Markdown links
  - keep navigation simple and stable
  - avoid formats that require proprietary tooling to read
- User documentation should also support product marketing where appropriate, including capability summaries, screenshots, feature highlights, and clear value statements for the plugin.

## Repo Shape Target

The intended solution should evolve toward:

- a VS Code extension shell in TypeScript
- one or more C# projects for core engines
- automated tests for both TypeScript and C# components
- reusable datasets for graph, compliance, and templates
- organized Markdown documentation under `docs/user/` and `docs/developer/`
- a GitHub Pages friendly documentation and marketing entry point rooted at `docs/index.md`

Keep new work aligned with the issue backlog in GitHub.
