# Issue 14 - Automated Tests, Packaging, And Release Readiness

## Summary

Closed the release-readiness gap by adding a curated smoke fixture, tightening the GitHub Actions workflows around the shared local validation commands, and documenting the maintainer release flow.

## Delivered

- curated fintech smoke fixture for analysis, compliance, and report generation
- end-to-end smoke test coverage in the C# test suite
- workflow assertions for CI and release command coverage
- CI and release workflow updates for `.NET`, `npm run verify`, and `npm run package:extension`
- dedicated release-readiness documentation for setup, debug, packaging, and known limitations
- VSIX packaging excludes repository-only fixture assets

## Validation

- `dotnet test core/ArchitectureStudio.sln`
- `npm run verify`
- `npm run package:extension`
