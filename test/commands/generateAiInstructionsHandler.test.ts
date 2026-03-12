import assert from "node:assert/strict";
import test from "node:test";

import generateAiInstructionsHandler from "../../src/commands/handlers/generateAiInstructionsHandler";

test("generateAiInstructions handler consumes ai instruction context and reports AGENTS output", async () => {
  const messages: string[] = [];
  const errors: string[] = [];
  const outputLines: string[] = [];
  let receivedWorkspacePath: string | undefined;
  let receivedProjectName: string | undefined;

  await generateAiInstructionsHandler({
    route: {
      id: "architectureStudio.generateAiInstructions",
      title: "Architecture Studio: Generate AI Instructions",
      handlerModule: "./handlers/generateAiInstructionsHandler"
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
      async getAiInstructionContext(workspacePath?: string) {
        receivedWorkspacePath = workspacePath;

        return {
          projectName: "Architecture Studio",
          targetKind: "GeneratedProject",
          projectSelection: {
            frontend: "react",
            backend: "aspnet-core",
            architecturePattern: "clean-architecture",
            ciCd: ["github-actions"],
            infrastructure: ["docker"],
            complianceTargets: ["hipaa"]
          },
          standards: [],
          complianceSummaries: [],
          findings: []
        };
      },
      async runAiInstructionGeneration(context) {
        receivedProjectName = context.projectName;

        return {
          generatedArtifacts: [
            {
              id: "artifact-agents",
              title: "AGENTS.md",
              kind: "AiInstructions",
              relativePath: "AGENTS.md"
            },
            {
              id: "artifact-ai-instructions",
              title: "AI Instructions",
              kind: "AiInstructions",
              relativePath: "docs/ai-instructions.md"
            }
          ],
          files: [
            {
              relativePath: "AGENTS.md",
              title: "AGENTS.md",
              kind: "AiInstructions",
              content: "# AGENTS"
            }
          ]
        };
      }
    }
  });

  assert.equal(receivedWorkspacePath, "C:/code/Playground/ARCHITECTURE_STUDIO");
  assert.equal(receivedProjectName, "Architecture Studio");
  assert.equal(errors.length, 0);
  assert.ok(outputLines.some((line) => line.includes("Target workspace: C:/code/Playground/ARCHITECTURE_STUDIO")));
  assert.ok(outputLines.some((line) => line.includes("Target kind: GeneratedProject")));
  assert.ok(outputLines.some((line) => line.includes("Generated artifacts: 2")));
  assert.ok(outputLines.some((line) => line.includes("AGENTS.md: generated")));
  assert.ok(messages.some((line) => line.includes("Generated 2 AI instruction artifacts")));
});

test("generateAiInstructions handler shows an error when no ai instruction context is available", async () => {
  const errors: string[] = [];

  await generateAiInstructionsHandler({
    route: {
      id: "architectureStudio.generateAiInstructions",
      title: "Architecture Studio: Generate AI Instructions",
      handlerModule: "./handlers/generateAiInstructionsHandler"
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
      },
      async getAiInstructionContext() {
        return undefined;
      }
    }
  });

  assert.equal(errors.length, 1);
  assert.match(errors[0], /instruction context/i);
});
