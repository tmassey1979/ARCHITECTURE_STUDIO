import assert from "node:assert/strict";
import test from "node:test";

import generateArchitectureHandler from "../../src/commands/handlers/generateArchitectureHandler";

test("generateArchitecture handler targets the active workspace and reports architecture recommendations", async () => {
  const messages: string[] = [];
  const errors: string[] = [];
  const outputLines: string[] = [];
  let evaluatedWorkspacePath: string | undefined;

  await generateArchitectureHandler({
    route: {
      id: "architectureStudio.generateArchitecture",
      title: "Architecture Studio: Generate Architecture",
      handlerModule: "./handlers/generateArchitectureHandler"
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
      async runArchitectureEvaluation(workspacePath: string) {
        evaluatedWorkspacePath = workspacePath;

        return {
          technologyEvaluation: {
            selectedNodes: [
              {
                id: "react",
                label: "React",
                category: "Framework"
              }
            ],
            missingRequirements: [],
            conflicts: [],
            recommendations: [
              {
                sourceNodeId: "react",
                nodeId: "rest-api",
                relationship: "PairsWith"
              }
            ]
          },
          findings: [
            {
              id: "missing-authentication",
              title: "Missing authentication",
              summary: "Protected flows do not yet have an authentication boundary.",
              severity: "Critical",
              risk: "Critical",
              remediation: {
                title: "Add an authentication provider",
                summary: "Introduce an identity provider before exposing protected features."
              },
              evidence: ["Authentication middleware or identity provider not configured."]
            }
          ]
        };
      }
    }
  });

  assert.equal(evaluatedWorkspacePath, "C:/code/Playground/ARCHITECTURE_STUDIO");
  assert.equal(errors.length, 0);
  assert.ok(outputLines.some((line) => line.includes("Target workspace: C:/code/Playground/ARCHITECTURE_STUDIO")));
  assert.ok(outputLines.some((line) => line.includes("Selected nodes: 1")));
  assert.ok(outputLines.some((line) => line.includes("Recommendations: 1")));
  assert.ok(outputLines.some((line) => line.includes("Findings: 1")));
  assert.ok(messages.some((line) => line.includes("Evaluated 1 architecture nodes")));
});

test("generateArchitecture handler shows an error when no workspace is open", async () => {
  const errors: string[] = [];

  await generateArchitectureHandler({
    route: {
      id: "architectureStudio.generateArchitecture",
      title: "Architecture Studio: Generate Architecture",
      handlerModule: "./handlers/generateArchitectureHandler"
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
