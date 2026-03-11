import assert from "node:assert/strict";
import fs from "node:fs";
import path from "node:path";
import test from "node:test";

import { createCommandExecutor, studioCommandRoutes } from "../../src/commands/commandRuntime";

test("studio command routes expose the required command identifiers", () => {
  const routeIds = studioCommandRoutes.map((route) => route.id).sort();

  assert.deepEqual(routeIds, [
    "architectureStudio.analyzeRepository",
    "architectureStudio.composeStandards",
    "architectureStudio.generateAiInstructions",
    "architectureStudio.generateArchitecture",
    "architectureStudio.generateProject",
    "architectureStudio.generateReports",
    "architectureStudio.openDashboard",
    "architectureStudio.validateRegulations"
  ]);
});

test("command executors lazy-load handlers on invocation", async () => {
  let loadCount = 0;
  const host = {
    showInformationMessage: async () => undefined,
    showErrorMessage: async () => undefined
  };
  const output = {
    lines: [] as string[],
    appendLine(line: string) {
      this.lines.push(line);
    }
  };

  const execute = createCommandExecutor(
    {
      id: "architectureStudio.test",
      title: "Architecture Studio: Test",
      handlerModule: "test/handler",
      loadHandler: async () => {
        loadCount += 1;
        return async () => undefined;
      }
    },
    host,
    output
  );

  assert.equal(loadCount, 0);
  await execute();
  assert.equal(loadCount, 1);
});

test("command executors surface failures through a consistent output and notification channel", async () => {
  const errors: string[] = [];
  const host = {
    showInformationMessage: async () => undefined,
    showErrorMessage: async (message: string) => {
      errors.push(message);
      return undefined;
    }
  };
  const output = {
    lines: [] as string[],
    appendLine(line: string) {
      this.lines.push(line);
    }
  };

  const execute = createCommandExecutor(
    {
      id: "architectureStudio.testFailure",
      title: "Architecture Studio: Test Failure",
      handlerModule: "test/failure",
      loadHandler: async () => async () => {
        throw new Error("boom");
      }
    },
    host,
    output
  );

  await execute();

  assert.equal(errors.length, 1);
  assert.match(errors[0], /architectureStudio\.testFailure/);
  assert.ok(output.lines.some((line) => line.includes("architectureStudio.testFailure")));
});

test("command metadata is documented for automation and contributors", () => {
  const commandDocs = fs.readFileSync(path.join(process.cwd(), "docs/developer/command-surface.md"), "utf8");

  for (const route of studioCommandRoutes) {
    assert.match(commandDocs, new RegExp(route.id.replace(".", "\\.")));
    assert.match(commandDocs, new RegExp(route.handlerModule.replace("/", "\\/")));
  }
});

test("package manifest command contributions match the centralized route catalog", () => {
  const manifest = JSON.parse(fs.readFileSync(path.join(process.cwd(), "package.json"), "utf8")) as {
    contributes: { commands: Array<{ command: string }> };
  };

  const manifestIds = manifest.contributes.commands.map((command) => command.command).sort();
  const routeIds = studioCommandRoutes.map((route) => route.id).sort();

  assert.deepEqual(manifestIds, routeIds);
});
