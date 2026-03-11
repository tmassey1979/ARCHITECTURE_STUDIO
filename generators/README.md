# Generators

This folder is reserved for project, pipeline, infrastructure, documentation, and AI-instruction generation assets and orchestration.

Current implementation ownership:

- C# generation engine code lives under `core/ArchitectureStudio.Core/Generation/`
- template datasets live under `templates/`
- TypeScript generation transport types live under `src/generators/`

Keep generation logic in C# and keep templates data-driven so later plugin packs can contribute new starter sets.
