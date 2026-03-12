import assert from "node:assert/strict";
import test from "node:test";

import { ArchitectureStudioDashboardSidebarProvider } from "../../src/dashboard/dashboardSidebarProvider";
import { createDashboardState, createEmptySharedContractPayload, createSampleSharedContractPayload } from "../../src/dashboard/dashboardState";

type Disposable = { dispose(): void };

class FakeWebview {
  public html = "";
  public options: unknown;
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

class FakeWebviewView {
  public readonly webview = new FakeWebview();
  public readonly viewType = "architectureStudio.dashboardView";
  public showCount = 0;
  public disposeListenerDisposals = 0;
  public visible = true;
  private readonly disposeListeners = new Set<() => void>();
  private readonly visibilityListeners = new Set<() => void>();

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

  onDidChangeVisibility(listener: () => void): Disposable {
    this.visibilityListeners.add(listener);

    return {
      dispose: () => {
        this.visibilityListeners.delete(listener);
      }
    };
  }

  show() {
    this.showCount += 1;
  }

  dispose() {
    for (const listener of [...this.disposeListeners]) {
      listener();
    }
  }
}

test("dashboard sidebar provider contributes the webview view and focuses it when requested", async () => {
  const executedCommands: string[] = [];
  const view = new FakeWebviewView();
  const provider = new ArchitectureStudioDashboardSidebarProvider({
    commands: {
      executeCommand(commandId: string) {
        executedCommands.push(commandId);
      }
    },
    extensionUri: {
      toString() {
        return "extension:/root";
      }
    },
    focusCommandId: "architectureStudio.dashboardView.focus",
    getState() {
      return createDashboardState(createSampleSharedContractPayload(), {
        workspaceLabel: "fintech-platform"
      });
    },
    output: {
      appendLine() {}
    },
    uri: {
      joinPath(base: { toString(): string }, ...paths: string[]) {
        return {
          toString() {
            return [base.toString(), ...paths].join("/");
          }
        };
      }
    }
  });

  await provider.show();
  assert.deepEqual(executedCommands, ["architectureStudio.dashboardView.focus"]);

  await provider.resolveWebviewView(view as never);
  assert.match(view.webview.html, /Architecture/);
  assert.match(view.webview.html, /dashboard\.css/);
  assert.match(view.webview.html, /dashboard\.js/);

  await provider.show();
  assert.equal(view.showCount, 1);
});

test("dashboard sidebar provider uses the typed message bridge and refreshes after command execution", async () => {
  let currentState = createDashboardState(
    createSampleSharedContractPayload({
      complianceSummaries: [
        {
          regulationId: "hipaa",
          regulationTitle: "HIPAA",
          scorePercentage: 68,
          coveredControls: 2,
          totalControls: 3
        }
      ]
    }),
    {
      workspaceLabel: "fintech-platform"
    }
  );
  const executedCommands: string[] = [];
  const outputLines: string[] = [];
  const view = new FakeWebviewView();
  const provider = new ArchitectureStudioDashboardSidebarProvider({
    commands: {
      async executeCommand(commandId: string) {
        executedCommands.push(commandId);
        currentState = createDashboardState(createEmptySharedContractPayload(), {
          workspaceLabel: "fintech-platform",
          subtitle: "Live workspace summary for fintech-platform."
        });
      }
    },
    extensionUri: {
      toString() {
        return "extension:/root";
      }
    },
    focusCommandId: "architectureStudio.dashboardView.focus",
    getState: async () => currentState,
    output: {
      appendLine(line: string) {
        outputLines.push(line);
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
    }
  });

  await provider.resolveWebviewView(view as never);

  view.webview.emit({ type: "dashboard.ready" });
  view.webview.emit({
    type: "dashboard.runCommand",
    commandId: "architectureStudio.generateReports"
  });
  await new Promise((resolve) => setImmediate(resolve));

  assert.equal(executedCommands[0], "architectureStudio.generateReports");
  assert.ok(view.webview.postedMessages.length >= 1);
  assert.equal(
    (view.webview.postedMessages.at(-1) as { state: { subtitle: string } }).state.subtitle,
    "Live workspace summary for fintech-platform."
  );

  view.dispose();
  assert.equal(view.webview.messageListenerDisposals, 1);
  assert.equal(view.disposeListenerDisposals, 1);
  assert.ok(outputLines.some((line) => line.includes("dashboard.runCommand")));
});
