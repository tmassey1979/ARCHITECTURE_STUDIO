import assert from "node:assert/strict";
import test from "node:test";

import {
  createDashboardState,
  createEmptySharedContractPayload,
  createSampleSharedContractPayload,
  isDashboardWebviewMessage
} from "../../src/dashboard/dashboardState";

test("dashboard state exposes the required top-level sections with explicit empty-state content", () => {
  const state = createDashboardState(createEmptySharedContractPayload(), {
    hasWorkspace: false
  });

  assert.deepEqual(
    state.sections.map((section) => section.title),
    ["Architecture", "Standards", "Compliance", "Reports", "Repository Analysis"]
  );

  const compliance = state.sections.find((section) => section.id === "compliance");
  const repositoryAnalysis = state.sections.find((section) => section.id === "repository-analysis");

  assert.ok(compliance);
  assert.ok(repositoryAnalysis);
  assert.ok(compliance.cards.length > 0, "Expected compliance summary cards.");
  assert.ok(compliance.panels.some((panel) => panel.items.some((item) => item.includes("Open a workspace folder"))));
  assert.ok(
    repositoryAnalysis.panels.some((panel) => panel.items.some((item) => item.includes("Open a workspace folder"))),
    "Expected explicit repository-analysis empty-state panels."
  );
});

test("dashboard state can project live shared-contract payloads into compliance and report sections", () => {
  const payload = createSampleSharedContractPayload({
    complianceSummaries: [
      {
        regulationId: "hipaa",
        regulationTitle: "HIPAA",
        scorePercentage: 72,
        coveredControls: 5,
        totalControls: 7
      }
    ],
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
  const state = createDashboardState(payload, {
    hasWorkspace: true,
    workspaceLabel: "fintech-platform"
  });
  const compliance = state.sections.find((section) => section.id === "compliance");
  const reports = state.sections.find((section) => section.id === "reports");

  assert.ok(compliance);
  assert.ok(reports);
  assert.ok(compliance.cards.some((card) => card.title === "HIPAA" && card.value === "72%"));
  assert.ok(compliance.panels.some((panel) => panel.items.some((item) => item.includes("Secrets committed"))));
  assert.ok(reports.panels.some((panel) => panel.items.some((item) => item.includes(payload.reports[0].title))));
});

test("dashboard state can surface external package load status in a user-visible panel", () => {
  const state = createDashboardState(createSampleSharedContractPayload(), {
    hasWorkspace: true,
    workspaceLabel: "fintech-platform",
    externalPackageStatuses: [
      {
        packageId: "aws-architecture-pack",
        status: "Loaded",
        message: "Loaded standards, templates, and graph contributions.",
        contributionKinds: ["Standards", "Templates", "Graph"]
      },
      {
        packageId: "banking-compliance-pack",
        status: "Invalid",
        message: "controls/missing.json was not found.",
        contributionKinds: ["Compliance"]
      }
    ]
  });
  const standards = state.sections.find((section) => section.id === "standards");

  assert.ok(standards);
  assert.ok(standards.cards.some((card) => card.title === "External Packs" && card.value === "2"));
  assert.ok(standards.panels.some((panel) => panel.items.some((item) => item.includes("aws-architecture-pack"))));
  assert.ok(standards.panels.some((panel) => panel.items.some((item) => item.includes("banking-compliance-pack"))));
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
