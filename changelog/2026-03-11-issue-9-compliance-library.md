# 2026-03-11 Issue 9 Compliance Library

## Summary

- expanded the compliance data from a small seed bundle into a broader regulation and control library
- added regulation modules for the required baseline and expanded frameworks, including PIPEDA
- expanded the control taxonomy to cover the required named controls plus additional supporting controls
- updated the compliance catalog loader to validate schema completeness and direct engine consumption
- added library-focused user and developer documentation

## Validation

- `dotnet test core/ArchitectureStudio.sln`
- `npm run verify`
- `npm run package:extension`
