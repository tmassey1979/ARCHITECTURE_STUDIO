# Dashboard Webview

## Purpose

The dashboard webview is the first shared UI shell for Architecture Studio. It gives the extension one place to surface:

- architecture state
- standards composition inputs
- compliance summaries and findings
- generated reports and artifacts
- repository analysis evidence

## Host And Client Boundary

The extension host owns sidebar-view lifecycle and data projection:

- `src/dashboard/dashboardSidebarProvider.ts` manages the contributed sidebar webview, focus behavior, visibility refresh, and typed message routing
- `src/dashboard/registerDashboardSidebar.ts` wires the provider into the contributed `architectureStudio.dashboardView`
- `src/dashboard/dashboardData.ts` aggregates the live workspace snapshot from the existing command-service engine calls
- `src/dashboard/dashboardState.ts` projects shared-contract payloads into a dashboard-oriented DTO
- `src/dashboard/dashboardHtml.ts` renders the initial HTML shell and references the packaged assets

The webview client owns presentation and interaction:

- `media/dashboard/dashboard.js` receives typed state updates and posts typed command requests back to the host
- `media/dashboard/dashboard.css` defines the dashboard visual system and responsive layout

## Message Bridge

Host to webview:

- `dashboard.stateChanged`

Webview to host:

- `dashboard.ready`
- `dashboard.runCommand`

The bridge is intentionally small. The webview consumes DTOs only and does not know about engine internals.

## Lifecycle Rules

- the dashboard is hosted in a contributed Activity Bar container rather than a free-floating editor panel
- only one contributed dashboard view is active at a time
- `Architecture Studio: Open Dashboard` focuses the contributed sidebar view
- disposing the view clears message and visibility subscriptions so handlers do not accumulate
- the provider can post refreshed state when the view is shown again
- dashboard-triggered commands refresh the state after execution so the view does not keep stale data
- async state refreshes are versioned so older responses do not overwrite newer ones

## Workspace Trust

The extension manifest now declares `capabilities.untrustedWorkspaces.supported: true` so the contributed Activity Bar view can activate in VS Code Restricted Mode. This is safe for the current product boundary because the extension executes only packaged binaries and reads workspace files locally; it does not execute workspace-provided code as part of dashboard activation.

## Live Snapshot Model

The live dashboard snapshot is assembled in TypeScript as a thin orchestration layer over the C# engines:

- repository analysis drives detected signals and sensitive-data summaries
- standards composition drives the standards section
- architecture evaluation drives graph nodes, edges, and architecture findings
- compliance evaluation drives score cards and remediation findings
- report generation drives the report artifact list
- project generation and AI-instruction generation contribute generated deliverables

The default state is now an explicit empty workspace state rather than a shipped sample payload.

## Asset Packaging

Dashboard assets live under `media/dashboard/` so they ship directly inside the VSIX and work in local extension debug sessions without a separate frontend bundle pipeline.

Illustrated screenshot asset:

- `docs/assets/dashboard/live-dashboard-fintech.svg`

## Testing

The dashboard test surface now covers:

- manifest contribution for the Activity Bar container and sidebar view
- sidebar-provider registration wiring
- required dashboard sections
- live workspace snapshot projection
- explicit no-workspace empty states
- typed message validation
- sidebar focus, dispose/reopen behavior, and post-command refresh
- packaged asset and documentation presence
