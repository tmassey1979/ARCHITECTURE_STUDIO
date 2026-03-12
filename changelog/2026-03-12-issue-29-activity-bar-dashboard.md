# Issue 29: Activity Bar Dashboard

## Summary

- added an `Architecture Studio` Activity Bar container with a sidebar-hosted dashboard webview
- registered a sidebar provider that reuses the existing dashboard HTML, state projection, and typed message bridge
- changed `Architecture Studio: Open Dashboard` to focus the contributed sidebar view instead of opening a separate panel
- added manifest, registration, and sidebar-provider regression tests plus user and developer documentation updates
