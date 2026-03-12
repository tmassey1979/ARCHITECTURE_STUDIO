# AI Instructions

## Purpose

Issue `#12` adds the first AI-instruction generation engine for Architecture Studio. The goal is to generate structured AI guidance and an `AGENTS.md` file from the same project profile, standards, and compliance context already owned elsewhere in the system.

The story delivers:

- a C# AI-instruction generation engine
- generated `AGENTS.md` output
- generated `docs/ai-instructions.md` output
- a workspace-aware `Generate AI Instructions` command boundary

## Engine Files

- `core/ArchitectureStudio.Core/Ai/AiInstructionGenerationModels.cs`
- `core/ArchitectureStudio.Core/Ai/AiInstructionGenerationEngine.cs`

## Input Composition

The generator takes structured context instead of rendering from a static file:

- project selection profile
- standards definitions
- compliance summaries
- findings
- target kind (`GeneratedProject` or `AnalyzedRepository`)

That keeps the generated guidance aligned to canonical standards and compliance outputs rather than drifting into a second hand-maintained ruleset.

## Output Shape

Current outputs:

- `AGENTS.md`
- `docs/ai-instructions.md`

The generated `AGENTS.md` includes the required sections from the story and the spec:

- architecture rules
- coding standards
- devops rules
- compliance requirements

## TypeScript Boundary

TypeScript owns:

- transport models in `src/ai/aiInstructionGeneration.ts`
- the `Generate AI Instructions` command handler

The command resolves workspace context when available, requests AI-instruction context through a service boundary, and reports whether `AGENTS.md` was generated.

## Testing

Issue `#12` is covered by:

- `core/ArchitectureStudio.Core.Tests/AiInstructionGenerationEngineTests.cs`
  - required section coverage
  - profile-derived content
  - deterministic output
- `test/commands/generateAiInstructionsHandler.test.ts`
  - context-driven command behavior
  - missing-context error handling
- `test/ai/aiInstructionArtifacts.test.ts`
  - transport and documentation artifacts
