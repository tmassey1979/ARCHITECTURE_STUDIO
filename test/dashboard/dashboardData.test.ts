import assert from "node:assert/strict";
import test from "node:test";

import type { StudioCommandServices } from "../../src/commands/commandRuntime";
import { createLiveDashboardState } from "../../src/dashboard/dashboardData";

test("live dashboard state aggregates workspace-backed engine output", async () => {
  const services: StudioCommandServices = {
    getWorkspaceFolder() {
      return "C:/workspace/fintech-platform";
    },
    async runRepositoryAnalysis() {
      return {
        signals: [
          {
            id: "react",
            label: "React",
            category: "Framework",
            confidence: 0.95,
            evidence: ["Detected React dependency."],
            affectedPaths: ["clients/portal/package.json"]
          },
          {
            id: "github-actions",
            label: "GitHub Actions",
            category: "CiCd",
            confidence: 0.99,
            evidence: ["Detected GitHub Actions workflow file."],
            affectedPaths: [".github/workflows/ci.yml"]
          }
        ],
        sensitiveData: [
          {
            category: "Financial",
            confidence: 0.92,
            evidence: ["Financial data indicator matched 'creditCard' in src/Web/appsettings.json."],
            affectedPaths: ["src/Web/appsettings.json"]
          }
        ]
      };
    },
    async runStandardsComposition() {
      return {
        standards: [
          {
            definition: {
              id: "react",
              title: "React Frontend Standard",
              category: "Frontend",
              summary: "Use React for the customer portal.",
              tags: ["react", "frontend"]
            },
            source: {
              packageId: "core",
              packageVersion: "1.0.0",
              sourcePath: "standards/frontend/react.json",
              sourceTitle: "React"
            },
            selectionReasons: ["Detected workspace frontend."]
          }
        ],
        consumerHints: []
      };
    },
    async runArchitectureEvaluation() {
      return {
        technologyEvaluation: {
          selectedNodes: [
            {
              id: "react",
              label: "React",
              category: "Framework"
            },
            {
              id: "aspnet-core",
              label: "ASP.NET Core",
              category: "Framework"
            }
          ],
          missingRequirements: [
            {
              sourceNodeId: "react",
              requiredNodeId: "typescript"
            }
          ],
          conflicts: [],
          recommendations: [
            {
              sourceNodeId: "react",
              nodeId: "vite",
              relationship: "RecommendedWith"
            }
          ]
        },
        findings: [
          {
            id: "missing-auth",
            title: "Missing authentication",
            summary: "Authentication middleware is not configured.",
            severity: "High",
            risk: "High",
            remediation: {
              title: "Configure authentication",
              summary: "Add authentication middleware to the host pipeline."
            },
            evidence: ["src/Api/Program.cs"]
          }
        ]
      };
    },
    async runComplianceEvaluation() {
      return {
        summaries: [
          {
            regulationId: "pci-dss",
            regulationTitle: "PCI DSS",
            scorePercentage: 60,
            coveredControls: 3,
            totalControls: 5
          }
        ],
        findings: [
          {
            id: "pci-logging",
            title: "Audit logging gap",
            summary: "Audit logging coverage is incomplete.",
            severity: "Medium",
            risk: "Medium",
            remediation: {
              title: "Expand audit logging",
              summary: "Capture payment authorization and admin actions."
            },
            evidence: ["src/Infrastructure/Payments/GatewayClient.cs"]
          }
        ]
      };
    },
    async getProjectSelection() {
      return {
        frontend: "react",
        backend: "aspnet-core",
        architecturePattern: "clean-architecture",
        ciCd: ["github-actions"],
        infrastructure: ["docker"],
        complianceTargets: ["pci-dss"]
      };
    },
    async runProjectGeneration() {
      return {
        templateIds: ["frontend-react", "backend-aspnet-core"],
        generatedArtifacts: [
          {
            id: "docs-readme-md",
            title: "Documentation Layout",
            kind: "Documentation",
            relativePath: "docs/README.md"
          }
        ],
        files: []
      };
    },
    async runReportGeneration() {
      return {
        reportArtifacts: [
          {
            id: "architecture-report-md",
            title: "architecture-report",
            format: "Markdown",
            relativePath: "reports/architecture-report.md"
          }
        ],
        files: [],
        pdfFallbackUsed: true
      };
    },
    async getAiInstructionContext() {
      return {
        projectName: "Fintech Platform",
        targetKind: "AnalyzedRepository",
        projectSelection: {
          frontend: "react",
          backend: "aspnet-core",
          architecturePattern: "clean-architecture",
          ciCd: ["github-actions"],
          infrastructure: ["docker"],
          complianceTargets: ["pci-dss"]
        },
        standards: [],
        complianceSummaries: [],
        findings: []
      };
    },
    async runAiInstructionGeneration() {
      return {
        generatedArtifacts: [
          {
            id: "agents-md",
            title: "AGENTS.md",
            kind: "AiInstructions",
            relativePath: "AGENTS.md"
          }
        ],
        files: []
      };
    }
  };

  const state = await createLiveDashboardState(services);

  assert.equal(state.subtitle, "Live workspace summary for fintech-platform.");

  const architecture = state.sections.find((section) => section.id === "architecture");
  const compliance = state.sections.find((section) => section.id === "compliance");
  const reports = state.sections.find((section) => section.id === "reports");
  const repositoryAnalysis = state.sections.find((section) => section.id === "repository-analysis");

  assert.ok(architecture);
  assert.ok(compliance);
  assert.ok(reports);
  assert.ok(repositoryAnalysis);
  assert.ok(architecture.cards.some((card) => card.title === "Target Pattern" && card.value === "clean-architecture"));
  assert.ok(architecture.panels.some((panel) => panel.items.some((item) => item.includes("React"))));
  assert.ok(compliance.cards.some((card) => card.title === "PCI DSS" && card.value === "60%"));
  assert.ok(compliance.panels.some((panel) => panel.items.some((item) => item.includes("Audit logging gap"))));
  assert.ok(reports.panels.some((panel) => panel.items.some((item) => item.includes("reports/architecture-report.md"))));
  assert.ok(reports.panels.some((panel) => panel.items.some((item) => item.includes("AGENTS.md"))));
  assert.ok(
    repositoryAnalysis.panels.some((panel) => panel.items.some((item) => item.includes("React (Framework)")))
  );
  assert.ok(
    repositoryAnalysis.panels.some((panel) => panel.items.some((item) => item.includes("Financial data")))
  );
});

test("live dashboard state returns an explicit empty state when no workspace is available", async () => {
  let serviceCalls = 0;
  const state = await createLiveDashboardState({
    getWorkspaceFolder() {
      return undefined;
    },
    async runRepositoryAnalysis() {
      serviceCalls += 1;
      return { signals: [], sensitiveData: [] };
    }
  });

  assert.equal(serviceCalls, 0);
  assert.equal(state.subtitle, "Open a workspace folder to load live Architecture Studio data.");

  const architecture = state.sections.find((section) => section.id === "architecture");
  const standards = state.sections.find((section) => section.id === "standards");
  const repositoryAnalysis = state.sections.find((section) => section.id === "repository-analysis");

  assert.ok(architecture);
  assert.ok(standards);
  assert.ok(repositoryAnalysis);
  assert.ok(architecture.panels.some((panel) => panel.items.some((item) => item.includes("Open a workspace folder"))));
  assert.ok(standards.panels.some((panel) => panel.items.some((item) => item.includes("Open a workspace folder"))));
  assert.ok(
    repositoryAnalysis.panels.some((panel) => panel.items.some((item) => item.includes("Open a workspace folder")))
  );
});
