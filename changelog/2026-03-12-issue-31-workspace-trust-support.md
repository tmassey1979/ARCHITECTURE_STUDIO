# 2026-03-12 Issue 31 Workspace Trust Support

## Summary

- declared Restricted Mode support in the VS Code extension manifest
- added manifest regression coverage for untrusted workspace capability
- documented how Workspace Trust affects the dashboard sidebar

## Validation

- `npm test -- --test test/dashboard/dashboardSidebarManifest.test.ts`
- `npm run verify`
- `dotnet test core/ArchitectureStudio.sln`
- `npm run package:extension`
- `code --install-extension .\\architecture-studio-0.1.0.vsix --force`
