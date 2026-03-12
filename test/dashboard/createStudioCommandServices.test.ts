import assert from "node:assert/strict";
import test from "node:test";

import { createStudioCommandServices } from "../../src/dashboard/createStudioCommandServices";

test("createStudioCommandServices delegates workspace-aware operations to the core CLI bridge", async () => {
  const invokedCommands: string[] = [];
  const services = createStudioCommandServices({
    commands: {},
    coreCli: {
      async analyzeRepository(workspacePath) {
        invokedCommands.push(`analyze:${workspacePath}`);
        return { signals: [], sensitiveData: [] };
      },
      async composeStandards(workspacePath) {
        invokedCommands.push(`standards:${workspacePath}`);
        return { standards: [], consumerHints: [] };
      },
      async evaluateArchitecture(workspacePath) {
        invokedCommands.push(`architecture:${workspacePath}`);
        return {
          technologyEvaluation: {
            selectedNodes: [],
            missingRequirements: [],
            conflicts: [],
            recommendations: []
          },
          findings: []
        };
      },
      async validateRegulations(workspacePath) {
        invokedCommands.push(`compliance:${workspacePath}`);
        return { summaries: [], findings: [] };
      },
      async inferProjectSelection(workspacePath) {
        invokedCommands.push(`selection:${workspacePath}`);
        return {
          frontend: "react",
          backend: "aspnet-core",
          architecturePattern: "clean-architecture",
          ciCd: ["github-actions"],
          infrastructure: ["docker"],
          complianceTargets: ["pci-dss"]
        };
      },
      async generateProject(selection) {
        invokedCommands.push(`project:${selection.frontend}`);
        return { templateIds: [], generatedArtifacts: [], files: [] };
      },
      async generateReports(workspacePath) {
        invokedCommands.push(`reports:${workspacePath}`);
        return { reportArtifacts: [], files: [], pdfFallbackUsed: true };
      },
      async buildAiInstructionRequest(workspacePath) {
        invokedCommands.push(`ai-context:${workspacePath}`);
        return {
          projectName: "Architecture Studio",
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
      async generateAiInstructions(request) {
        invokedCommands.push(`ai:${request.projectName}`);
        return { generatedArtifacts: [], files: [] };
      }
    },
    extensionPath: "C:/code/Playground/ARCHITECTURE_STUDIO",
    extensionUri: {
      toString() {
        return "file:///architecture-studio";
      }
    },
    output: {
      appendLine() {}
    },
    uri: {
      joinPath(base, ...paths) {
        return {
          toString() {
            return [base.toString(), ...paths].join("/");
          }
        };
      }
    },
    viewColumn: 1,
    window: {
      createWebviewPanel() {
        throw new Error("dashboard should not be created in this test");
      }
    },
    workspace: {
      getFirstWorkspaceFolderPath() {
        return "C:/workspace/sample";
      }
    }
  });

  await services.runRepositoryAnalysis?.("C:/workspace/sample");
  await services.runStandardsComposition?.("C:/workspace/sample");
  await services.runArchitectureEvaluation?.("C:/workspace/sample");
  await services.runComplianceEvaluation?.("C:/workspace/sample");
  const selection = await services.getProjectSelection?.();
  await services.runProjectGeneration?.(selection!);
  await services.runReportGeneration?.("C:/workspace/sample");
  const aiContext = await services.getAiInstructionContext?.("C:/workspace/sample");
  await services.runAiInstructionGeneration?.(aiContext!);

  assert.deepEqual(invokedCommands, [
    "analyze:C:/workspace/sample",
    "standards:C:/workspace/sample",
    "architecture:C:/workspace/sample",
    "compliance:C:/workspace/sample",
    "selection:C:/workspace/sample",
    "project:react",
    "reports:C:/workspace/sample",
    "ai-context:C:/workspace/sample",
    "ai:Architecture Studio"
  ]);
});
