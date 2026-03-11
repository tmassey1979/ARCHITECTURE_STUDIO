import assert from "node:assert/strict";
import test from "node:test";

import analyzeRepositoryHandler from "../../src/commands/handlers/analyzeRepositoryHandler";

test("analyzeRepository handler targets the active workspace and reports the returned summary", async () => {
  const messages: string[] = [];
  const errors: string[] = [];
  const outputLines: string[] = [];
  let analyzedPath: string | undefined;

  await analyzeRepositoryHandler({
    route: {
      id: "architectureStudio.analyzeRepository",
      title: "Architecture Studio: Analyze Repository",
      handlerModule: "./handlers/analyzeRepositoryHandler"
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
      async runRepositoryAnalysis(workspacePath: string) {
        analyzedPath = workspacePath;

        return {
          signals: [
            {
              id: "aspnet-core",
              label: "ASP.NET Core",
              category: "Framework",
              confidence: 0.98,
              evidence: ["Architecture.Api.csproj uses Microsoft.NET.Sdk.Web"],
              affectedPaths: ["src/api/Architecture.Api.csproj"]
            }
          ],
          sensitiveData: [
            {
              category: "Personal",
              confidence: 0.9,
              evidence: ["customerEmail key found in appsettings.json"],
              affectedPaths: ["src/Web/appsettings.json"]
            }
          ]
        };
      }
    }
  });

  assert.equal(analyzedPath, "C:/code/Playground/ARCHITECTURE_STUDIO");
  assert.equal(errors.length, 0);
  assert.ok(outputLines.some((line) => line.includes("Target workspace: C:/code/Playground/ARCHITECTURE_STUDIO")));
  assert.ok(outputLines.some((line) => line.includes("Signals: 1")));
  assert.ok(outputLines.some((line) => line.includes("Sensitive data classifications: 1")));
  assert.ok(messages.some((line) => line.includes("Analyzed 1 repository signals")));
});

test("analyzeRepository handler shows an error when no workspace is open", async () => {
  const errors: string[] = [];

  await analyzeRepositoryHandler({
    route: {
      id: "architectureStudio.analyzeRepository",
      title: "Architecture Studio: Analyze Repository",
      handlerModule: "./handlers/analyzeRepositoryHandler"
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
