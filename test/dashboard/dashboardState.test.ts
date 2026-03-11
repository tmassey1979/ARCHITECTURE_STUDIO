import assert from "node:assert/strict";
import test from "node:test";

import {
  createDashboardState,
  createPlaceholderSharedContractPayload,
  isDashboardWebviewMessage
} from "../../src/dashboard/dashboardState";

test("dashboard state exposes the required top-level sections with placeholder contract-backed content", () => {
  const state = createDashboardState();

  assert.deepEqual(
    state.sections.map((section) => section.title),
    ["Architecture", "Standards", "Compliance", "Reports", "Repository Analysis"]
  );

  const compliance = state.sections.find((section) => section.id === "compliance");
  const repositoryAnalysis = state.sections.find((section) => section.id === "repository-analysis");

  assert.ok(compliance);
  assert.ok(repositoryAnalysis);
  assert.ok(compliance.cards.length > 0, "Expected compliance summary cards.");
  assert.ok(compliance.panels.some((panel) => panel.items.length > 0), "Expected compliance finding panels.");
  assert.ok(
    repositoryAnalysis.panels.some((panel) => panel.items.length > 0),
    "Expected repository analysis placeholder panels."
  );
});

test("dashboard state can project live shared-contract payloads into compliance and report sections", () => {
  const payload = createPlaceholderSharedContractPayload({
    findings: [
      {
        id: "finding-live-1",
        title: "Secrets committed to source",
        summary: "A credential was found in a tracked configuration file.",
        severity: "Critical",
        risk: "High",
        remediation: {
          title: "Rotate and remove secret",
          summary: "Revoke the current secret, move it into secure storage, and scrub the commit history."
        },
        evidence: ["infra/appsettings.json"]
      }
    ]
  });
  const state = createDashboardState(payload);
  const compliance = state.sections.find((section) => section.id === "compliance");
  const reports = state.sections.find((section) => section.id === "reports");

  assert.ok(compliance);
  assert.ok(reports);
  assert.ok(compliance.panels.some((panel) => panel.items.some((item) => item.includes("Secrets committed"))));
  assert.ok(reports.panels.some((panel) => panel.items.some((item) => item.includes(payload.reports[0].title))));
});

test("dashboard webview message guard accepts only supported typed messages", () => {
  assert.equal(isDashboardWebviewMessage({ type: "dashboard.ready" }), true);
  assert.equal(
    isDashboardWebviewMessage({
      type: "dashboard.runCommand",
      commandId: "architectureStudio.composeStandards"
    }),
    true
  );
  assert.equal(isDashboardWebviewMessage({ type: "dashboard.runCommand" }), false);
  assert.equal(isDashboardWebviewMessage({ type: "unknown" }), false);
});
