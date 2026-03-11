# Dashboard Webview

## Purpose

The dashboard webview is the first shared UI shell for Architecture Studio. It gives the extension one place to surface:

- architecture state
- standards composition inputs
- compliance summaries and findings
- generated reports and artifacts
- repository analysis evidence

## Host And Client Boundary

The extension host owns panel lifecycle and data projection:

- `src/dashboard/dashboardController.ts` manages the single active panel, reopen behavior, and typed message routing
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

- only one dashboard panel is active at a time
- reopening reveals the existing panel instead of creating duplicates
- disposing the panel clears the message and dispose subscriptions so handlers do not accumulate
- the controller can post refreshed state when the panel is revealed again

## Asset Packaging

Dashboard assets live under `media/dashboard/` so they ship directly inside the VSIX and work in local extension debug sessions without a separate frontend bundle pipeline.

## Testing

Issue `#4` adds tests for:

- required dashboard sections
- shared-contract-backed placeholder and live projection
- typed message validation
- panel reuse and dispose/reopen behavior
- packaged asset and documentation presence
