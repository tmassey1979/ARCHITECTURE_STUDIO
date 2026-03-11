import assert from "node:assert/strict";
import test from "node:test";

import validateRegulationsHandler from "../../src/commands/handlers/validateRegulationsHandler";

test("validateRegulations handler targets the active workspace and reports compliance summaries", async () => {
  const messages: string[] = [];
  const errors: string[] = [];
  const outputLines: string[] = [];
  let evaluatedWorkspacePath: string | undefined;

  await validateRegulationsHandler({
    route: {
      id: "architectureStudio.validateRegulations",
      title: "Architecture Studio: Validate Regulations",
      handlerModule: "./handlers/validateRegulationsHandler"
    },
    host: {
      async showInformationMessage(message: string) {
        messages.push(message);
        return undefined;
      },
      async showErrorMessage(message: string) {
        errors.push(message);
        return undefined;
      }
    },
    output: {
      appendLine(line: string) {
        outputLines.push(line);
      }
    },
    arguments: [],
    services: {
      async getWorkspaceFolder() {
        return "C:/code/Playground/ARCHITECTURE_STUDIO";
      },
      async runComplianceEvaluation(workspacePath: string) {
        evaluatedWorkspacePath = workspacePath;

        return {
          summaries: [
            {
              regulationId: "hipaa",
              regulationTitle: "HIPAA",
              scorePercentage: 72,
              coveredControls: 5,
              totalControls: 7
            },
            {
              regulationId: "soc2",
              regulationTitle: "SOC 2",
              scorePercentage: 100,
              coveredControls: 3,
              totalControls: 3
            }
          ],
          findings: [
            {
              id: "missing-control-hipaa-audit-logging",
              title: "HIPAA missing control: Audit Logging",
              summary: "Audit logging is required for HIPAA and is not fully covered.",
              severity: "High",
              risk: "High",
              remediation: {
                title: "Implement audit logging",
                summary: "Add auditable tracking for protected data access."
              },
              evidence: ["src/Web/appsettings.json"]
            }
          ]
        };
      }
    }
  });

  assert.equal(evaluatedWorkspacePath, "C:/code/Playground/ARCHITECTURE_STUDIO");
  assert.equal(errors.length, 0);
  assert.ok(outputLines.some((line) => line.includes("Target workspace: C:/code/Playground/ARCHITECTURE_STUDIO")));
  assert.ok(outputLines.some((line) => line.includes("HIPAA 72%")));
  assert.ok(outputLines.some((line) => line.includes("SOC 2 100%")));
  assert.ok(outputLines.some((line) => line.includes("Findings: 1")));
  assert.ok(messages.some((line) => line.includes("Evaluated 2 applicable regulations")));
});

test("validateRegulations handler shows an error when no workspace is open", async () => {
  const errors: string[] = [];

  await validateRegulationsHandler({
    route: {
      id: "architectureStudio.validateRegulations",
      title: "Architecture Studio: Validate Regulations",
      handlerModule: "./handlers/validateRegulationsHandler"
    },
    host: {
      async showInformationMessage() {
        return undefined;
      },
      async showErrorMessage(message: string) {
        errors.push(message);
        return undefined;
      }
    },
    output: {
      appendLine() {}
    },
    arguments: [],
    services: {
      async getWorkspaceFolder() {
        return undefined;
      }
    }
  });

  assert.equal(errors.length, 1);
  assert.match(errors[0], /open workspace/i);
});
