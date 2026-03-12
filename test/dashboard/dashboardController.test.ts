import assert from "node:assert/strict";
import test from "node:test";

import { ArchitectureStudioDashboardController } from "../../src/dashboard/dashboardController";
import { createEmptySharedContractPayload, createSampleSharedContractPayload, createDashboardState } from "../../src/dashboard/dashboardState";

type Disposable = { dispose(): void };

class FakeWebview {
  public html = "";
  public readonly cspSource = "csp-source";
  public readonly postedMessages: unknown[] = [];
  public messageListenerDisposals = 0;
  private readonly listeners = new Set<(message: unknown) => void>();

  asWebviewUri(resource: { toString(): string }) {
    return {
      toString() {
        return `webview:${resource.toString()}`;
      }
    };
  }

  postMessage(message: unknown) {
    this.postedMessages.push(message);
    return true;
  }

  onDidReceiveMessage(listener: (message: unknown) => void): Disposable {
    this.listeners.add(listener);

    return {
      dispose: () => {
        if (this.listeners.delete(listener)) {
          this.messageListenerDisposals += 1;
        }
      }
    };
  }

  emit(message: unknown) {
    for (const listener of this.listeners) {
      listener(message);
    }
  }
}

class FakePanel {
  public readonly webview = new FakeWebview();
  public revealCount = 0;
  public disposeListenerDisposals = 0;
  public disposed = false;
  private readonly disposeListeners = new Set<() => void>();

  reveal() {
    this.revealCount += 1;
  }

  onDidDispose(listener: () => void): Disposable {
    this.disposeListeners.add(listener);

    return {
      dispose: () => {
        if (this.disposeListeners.delete(listener)) {
          this.disposeListenerDisposals += 1;
        }
      }
    };
  }

  dispose() {
    this.disposed = true;

    for (const listener of [...this.disposeListeners]) {
      listener();
    }
  }
}

function createController() {
  const createdPanels: FakePanel[] = [];
  const executedCommands: string[] = [];
  const outputLines: string[] = [];

  const controller = new ArchitectureStudioDashboardController({
    extensionUri: {
      toString() {
        return "extension:/root";
      }
    },
    output: {
      appendLine(line: string) {
        outputLines.push(line);
      }
    },
    commands: {
      executeCommand(commandId: string) {
        executedCommands.push(commandId);
        return undefined;
      }
    },
    uri: {
      joinPath(base: { toString(): string }, ...paths: string[]) {
        return {
          toString() {
            return [base.toString(), ...paths].join("/");
          }
        };
      }
    },
    window: {
      createWebviewPanel() {
        const panel = new FakePanel();
        createdPanels.push(panel);
        return panel;
      }
    },
    viewColumn: 1,
    nonceFactory: () => "nonce-test"
  });

  return { controller, createdPanels, executedCommands, outputLines };
}

test("dashboard controller renders packaged assets and required sections, and reuses the active panel", async () => {
  const { controller, createdPanels } = createController();

  await controller.show();
  await controller.show();

  assert.equal(createdPanels.length, 1);
  assert.equal(createdPanels[0].revealCount, 1);
  assert.match(createdPanels[0].webview.html, /Architecture/);
  assert.match(createdPanels[0].webview.html, /Standards/);
  assert.match(createdPanels[0].webview.html, /Compliance/);
  assert.match(createdPanels[0].webview.html, /Reports/);
  assert.match(createdPanels[0].webview.html, /Repository Analysis/);
  assert.match(createdPanels[0].webview.html, /dashboard\.css/);
  assert.match(createdPanels[0].webview.html, /dashboard\.js/);
});

test("dashboard controller uses the typed message bridge and cleans up listeners across dispose and reopen", async () => {
  const { controller, createdPanels, executedCommands, outputLines } = createController();

  await controller.show();
  const firstPanel = createdPanels[0];

  firstPanel.webview.emit({ type: "dashboard.ready" });
  firstPanel.webview.emit({
    type: "dashboard.runCommand",
    commandId: "architectureStudio.composeStandards"
  });
  await new Promise((resolve) => setImmediate(resolve));

  assert.equal(firstPanel.webview.postedMessages.length, 1);
  assert.deepEqual(firstPanel.webview.postedMessages[0], {
    type: "dashboard.stateChanged",
    state: firstPanel.webview.postedMessages[0]["state"]
  });
  assert.equal(executedCommands[0], "architectureStudio.composeStandards");

  firstPanel.dispose();

  assert.equal(firstPanel.webview.messageListenerDisposals, 1);
  assert.equal(firstPanel.disposeListenerDisposals, 1);

  await controller.show();
  assert.equal(createdPanels.length, 2);
  assert.notEqual(createdPanels[1], firstPanel);
  assert.ok(outputLines.some((line) => line.includes("dashboard.runCommand")));
});

test("dashboard controller refreshes live state after a dashboard-triggered command", async () => {
  let currentState = createDashboardState(
    createSampleSharedContractPayload({
      complianceSummaries: [
        {
          regulationId: "pci-dss",
          regulationTitle: "PCI DSS",
          scorePercentage: 60,
          coveredControls: 3,
          totalControls: 5
        }
      ]
    }),
    {
      workspaceLabel: "fintech-platform"
    }
  );
  const createdPanels: FakePanel[] = [];

  const controller = new ArchitectureStudioDashboardController({
    extensionUri: {
      toString() {
        return "extension:/root";
      }
    },
    getState: async () => currentState,
    output: {
      appendLine() {}
    },
    commands: {
      async executeCommand() {
        currentState = createDashboardState(createEmptySharedContractPayload(), {
          workspaceLabel: "fintech-platform",
          subtitle: "Live workspace summary for fintech-platform."
        });
      }
    },
    uri: {
      joinPath(base: { toString(): string }, ...paths: string[]) {
        return {
          toString() {
            return [base.toString(), ...paths].join("/");
          }
        };
      }
    },
    window: {
      createWebviewPanel() {
        const panel = new FakePanel();
        createdPanels.push(panel);
        return panel;
      }
    },
    viewColumn: 1,
    nonceFactory: () => "nonce-test"
  });

  await controller.show();
  const panel = createdPanels[0];

  panel.webview.emit({
    type: "dashboard.runCommand",
    commandId: "architectureStudio.generateReports"
  });
  await new Promise((resolve) => setImmediate(resolve));

  assert.equal(panel.webview.postedMessages.length, 1);
  assert.equal(
    (panel.webview.postedMessages[0] as { state: { subtitle: string } }).state.subtitle,
    "Live workspace summary for fintech-platform."
  );
});
