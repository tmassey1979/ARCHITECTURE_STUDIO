# Issue 1 - Bootstrap the Architecture Studio Workspace

## Summary

Bootstrapped the initial Architecture Studio repository so it now has:

- a runnable VS Code extension shell in TypeScript
- a pinned .NET core workspace for future C# business logic
- TDD-first bootstrap tests
- baseline packaging and debug configuration
- local developer setup documentation

## Delivered

- Added a VS Code extension manifest and TypeScript scaffold:
  - `package.json`
  - `tsconfig.json`
  - `src/extension.ts`
  - `src/commands/*`
- Added baseline build, lint, test, and package scripts.
- Added the top-level product work areas required by the codex specification:
  - `analysis/`
  - `generators/`
  - `graph/`
  - `ui/`
  - `standards/`
  - `compliance/`
  - `templates/`
  - `reports/`
  - `changelog/`
- Added the initial C# solution and test project under `core/`.
- Added local debug configuration under `.vscode/`.
- Added bootstrap documentation in `docs/developer/local-development.md`.
- Added a failing-first bootstrap test suite in `test/bootstrap/workspace-bootstrap.test.mjs`.

## Validation

- `node --test`
- `npm run verify`
- `dotnet test core/ArchitectureStudio.sln`
- `npm run package:extension`

## Notes

- The bootstrap currently pins `.NET SDK 9.0.311` through `global.json` because that is the installed SDK available in this workspace today.
- `vsce package` succeeds. It emits a warning about a missing license file, which does not block packaging but should be addressed before public release.
