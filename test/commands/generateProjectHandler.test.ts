import assert from "node:assert/strict";
import test from "node:test";

import generateProjectHandler from "../../src/commands/handlers/generateProjectHandler";

test("generateProject handler consumes a selection and reports generated artifact counts", async () => {
  const messages: string[] = [];
  const errors: string[] = [];
  const outputLines: string[] = [];
  let receivedSelection:
    | {
        frontend: string;
        backend: string;
        architecturePattern: string;
        ciCd: readonly string[];
        infrastructure: readonly string[];
        complianceTargets: readonly string[];
      }
    | undefined;

  await generateProjectHandler({
    route: {
      id: "architectureStudio.generateProject",
      title: "Architecture Studio: Generate Project",
      handlerModule: "./handlers/generateProjectHandler"
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
      async getProjectSelection() {
        return {
          frontend: "react",
          backend: "aspnet-core",
          architecturePattern: "clean-architecture",
          ciCd: ["github-actions"],
          infrastructure: ["docker", "kubernetes"],
          complianceTargets: ["hipaa"]
        };
      },
      async runProjectGeneration(selection) {
        receivedSelection = selection;

        return {
          templateIds: ["frontend-react", "backend-aspnet-core", "pipeline-github-actions"],
          generatedArtifacts: [
            {
              id: "artifact-docs-architecture",
              title: "Architecture Stub",
              kind: "Documentation",
              relativePath: "docs/architecture/clean-architecture.md"
            }
          ],
          files: [
            {
              relativePath: "docs/architecture/clean-architecture.md",
              title: "Architecture Stub",
              kind: "Documentation",
              content: "# Clean Architecture"
            }
          ]
        };
      }
    }
  });

  assert.equal(receivedSelection?.frontend, "react");
  assert.equal(errors.length, 0);
  assert.ok(outputLines.some((line) => line.includes("Selection: react / aspnet-core / clean-architecture")));
  assert.ok(outputLines.some((line) => line.includes("Generated artifacts: 1")));
  assert.ok(outputLines.some((line) => line.includes("Template set: frontend-react, backend-aspnet-core, pipeline-github-actions")));
  assert.ok(messages.some((line) => line.includes("Generated 1 artifacts")));
});

test("generateProject handler shows an error when no generation selection is available", async () => {
  const errors: string[] = [];

  await generateProjectHandler({
    route: {
      id: "architectureStudio.generateProject",
      title: "Architecture Studio: Generate Project",
      handlerModule: "./handlers/generateProjectHandler"
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
      async getProjectSelection() {
        return undefined;
      }
    }
  });

  assert.equal(errors.length, 1);
  assert.match(errors[0], /selection/i);
});
