import assert from "node:assert/strict";
import test from "node:test";

import composeStandardsHandler from "../../src/commands/handlers/composeStandardsHandler";

test("composeStandards handler targets the active workspace and reports composed standards", async () => {
  const messages: string[] = [];
  const errors: string[] = [];
  const outputLines: string[] = [];
  let composedWorkspacePath: string | undefined;

  await composeStandardsHandler({
    route: {
      id: "architectureStudio.composeStandards",
      title: "Architecture Studio: Compose Standards",
      handlerModule: "./handlers/composeStandardsHandler"
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
      async runStandardsComposition(workspacePath: string) {
        composedWorkspacePath = workspacePath;

        return {
          standards: [
            {
              definition: {
                id: "react",
                title: "React",
                category: "Frontend",
                summary: "Use component-driven UI composition with explicit state boundaries.",
                tags: ["frontend"]
              },
              source: {
                packageId: "architecture-studio.seed",
                packageVersion: "1.0.0",
                sourcePath: "standards/packages/architecture-studio.seed.json",
                sourceTitle: "Architecture Studio Seed Library"
              },
              selectionReasons: ["Detected technology: React"]
            }
          ],
          consumerHints: [
            {
              workflow: "Dashboard",
              usage: "Render grouped standards summaries in the dashboard."
            }
          ]
        };
      }
    }
  });

  assert.equal(composedWorkspacePath, "C:/code/Playground/ARCHITECTURE_STUDIO");
  assert.equal(errors.length, 0);
  assert.ok(outputLines.some((line) => line.includes("Target workspace: C:/code/Playground/ARCHITECTURE_STUDIO")));
  assert.ok(outputLines.some((line) => line.includes("Standards: 1")));
  assert.ok(outputLines.some((line) => line.includes("Top standard: React")));
  assert.ok(messages.some((line) => line.includes("Composed 1 standards")));
});

test("composeStandards handler shows an error when no workspace is open", async () => {
  const errors: string[] = [];

  await composeStandardsHandler({
    route: {
      id: "architectureStudio.composeStandards",
      title: "Architecture Studio: Compose Standards",
      handlerModule: "./handlers/composeStandardsHandler"
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
