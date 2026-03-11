import assert from "node:assert/strict";
import test from "node:test";

import generateReportsHandler from "../../src/commands/handlers/generateReportsHandler";

test("generateReports handler targets the active workspace and reports export counts", async () => {
  const messages: string[] = [];
  const errors: string[] = [];
  const outputLines: string[] = [];
  let evaluatedWorkspacePath: string | undefined;

  await generateReportsHandler({
    route: {
      id: "architectureStudio.generateReports",
      title: "Architecture Studio: Generate Reports",
      handlerModule: "./handlers/generateReportsHandler"
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
      async runReportGeneration(workspacePath: string) {
        evaluatedWorkspacePath = workspacePath;

        return {
          pdfFallbackUsed: true,
          reportArtifacts: [
            {
              id: "report-architecture-markdown",
              title: "Architecture Report",
              format: "Markdown",
              relativePath: "reports/architecture-report.md"
            },
            {
              id: "report-compliance-json",
              title: "Compliance Report",
              format: "Json",
              relativePath: "reports/compliance-report.json"
            }
          ],
          files: [
            {
              relativePath: "reports/architecture-report.md",
              format: "Markdown",
              content: "# Architecture Report"
            }
          ]
        };
      }
    }
  });

  assert.equal(evaluatedWorkspacePath, "C:/code/Playground/ARCHITECTURE_STUDIO");
  assert.equal(errors.length, 0);
  assert.ok(outputLines.some((line) => line.includes("Target workspace: C:/code/Playground/ARCHITECTURE_STUDIO")));
  assert.ok(outputLines.some((line) => line.includes("Report artifacts: 2")));
  assert.ok(outputLines.some((line) => line.includes("PDF fallback: enabled")));
  assert.ok(messages.some((line) => line.includes("Generated 2 report artifacts")));
});

test("generateReports handler shows an error when no workspace is open", async () => {
  const errors: string[] = [];

  await generateReportsHandler({
    route: {
      id: "architectureStudio.generateReports",
      title: "Architecture Studio: Generate Reports",
      handlerModule: "./handlers/generateReportsHandler"
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
