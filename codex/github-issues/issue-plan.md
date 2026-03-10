# Architecture Studio Issue Seed Plan

Sources:

- `codex/studio.md`
- `codex/data.md`

This plan mirrors the `DragonEnvelopes` issue standard:

- Epic issue for the overall delivery
- Feature-story child issues with `Story`, `Acceptance Criteria`, `Dev Notes`, and `Lifecycle Checklist`
- Ordered dependencies called out in the epic and child issue notes

Implementation defaults for the whole backlog:

- Build with TDD.
- Prefer C#/.NET 10 for core engines and business logic where the VS Code runtime boundary allows it.
- Keep TypeScript focused on the extension host, webview, and thin orchestration layers.

## Issue Order

1. `Feature: Bootstrap the Architecture Studio VS Code extension workspace`
2. `Feature: Define shared contracts for findings, graphs, regulations, and generators`
3. `Feature: Register Architecture Studio commands and activation flow`
4. `Feature: Build the Architecture Studio dashboard webview shell`
5. `Feature: Implement the standards library and standards composition engine`
6. `Feature: Implement the technology graph and architecture reasoning engine`
7. `Feature: Implement repository analysis and sensitive data classification`
8. `Feature: Implement the compliance engine with scoring and remediation`
9. `Feature: Author the regulation and control libraries`
10. `Feature: Implement project, pipeline, and infrastructure generation`
11. `Feature: Implement report and documentation generation`
12. `Feature: Implement AI instruction and AGENTS.md generation`
13. `Feature: Implement the external standards package plugin system`
14. `Feature: Add automated tests, packaging, and release readiness`
15. `Epic: Deliver the Architecture Studio master build`

## Dependency Notes

- `Bootstrap` is the foundation for every other story.
- `Shared contracts` should land before deep engine work so findings, graph edges, controls, and generation outputs stay consistent.
- `Commands` and `Dashboard` can start after bootstrap, but they should consume the shared contracts instead of defining their own models.
- `Standards`, `Architecture`, `Analysis`, and `Compliance` are the core engines that feed the generator and reporting stories.
- `Regulation and control libraries` depend on the compliance engine contracts.
- `Project generation`, `Reports`, and `AI instructions` depend on the outputs of the standards, architecture, analysis, and compliance stories.
- `Plugin system` depends on stable standards, compliance, graph, and template contracts.
- `Tests and packaging` close the loop after the feature surface is in place.

## Dataset Coverage Notes

- `Feature #6` must cover the large curated technology graph dataset described in `codex/data.md`, including broad category coverage and relationship authoring at the stated scale.
- `Feature #9` must cover the expanded regulation dataset and control taxonomy from `codex/data.md`, including privacy-law coverage beyond the baseline `studio.md` list.
- `Feature #10` must cover the template dataset scale from `codex/data.md`, including 20+ architecture and delivery variants rather than a single starter template.
