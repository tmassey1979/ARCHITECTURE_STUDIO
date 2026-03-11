import assert from "node:assert/strict";
import test from "node:test";

import { registerArchitectureStudioCommands } from "../../src/commands/registerCommands";
import { studioCommandRoutes } from "../../src/commands/commandRuntime";

type Disposable = { dispose(): void };

function createContext() {
  return {
    subscriptions: [] as Disposable[]
  };
}

test("registerArchitectureStudioCommands wires the centralized route catalog through a shared output channel", async () => {
  const registered = new Map<string, () => Promise<void>>();
  const errorMessages: string[] = [];
  const outputLines: string[] = [];
  let dashboardOpenCount = 0;
  const context = createContext();

  registerArchitectureStudioCommands(
    context as never,
    {
      commands: {
        registerCommand(id: string, handler: () => Promise<void>): Disposable {
          registered.set(id, handler);
          return { dispose() {} };
        }
      },
      window: {
        createOutputChannel(): { appendLine(line: string): void; dispose(): void } {
          return {
            appendLine(line: string) {
              outputLines.push(line);
            },
            dispose() {}
          };
        },
        async showInformationMessage(): Promise<string | undefined> {
          return undefined;
        },
        async showErrorMessage(message: string): Promise<string | undefined> {
          errorMessages.push(message);
          return undefined;
        }
      }
    },
    {
      services: {
        async showDashboard() {
          dashboardOpenCount += 1;
        }
      }
    }
  );

  assert.equal(context.subscriptions.length, studioCommandRoutes.length + 1);
  assert.equal(registered.size, studioCommandRoutes.length);
  assert.deepEqual([...registered.keys()].sort(), studioCommandRoutes.map((route) => route.id).sort());
  assert.equal(outputLines.length, 0);

  await registered.get("architectureStudio.openDashboard")?.();

  assert.equal(errorMessages.length, 0);
  assert.equal(dashboardOpenCount, 1);
  assert.ok(outputLines.some((line) => line.includes("architectureStudio.openDashboard")));
});
