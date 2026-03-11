# 2026-03-11 Issue 8 Compliance Engine

## Summary

- added a seed compliance catalog for controls and regulations under `compliance/`
- added a deterministic C# compliance engine with regulation applicability, score calculation, and remediation findings
- added shared compliance summary contracts for dashboard and downstream consumers
- replaced the placeholder Validate Regulations handler with a workspace-aware command boundary
- added user and developer documentation for compliance validation

## Validation

- `dotnet test core/ArchitectureStudio.sln`
- `npm run verify`
- `npm run package:extension`
