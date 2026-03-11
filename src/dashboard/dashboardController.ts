import type { StudioCommandOutput } from "../commands/commandRuntime";
import { createDashboardState, createDashboardStateMessage, isDashboardWebviewMessage, type DashboardState } from "./dashboardState";
import { renderDashboardHtml, type DashboardHtmlWebview, type DashboardResourceUri, type DashboardUriApi } from "./dashboardHtml";

export interface DashboardDisposable {
  dispose(): void;
}

export interface DashboardWebview extends DashboardHtmlWebview {
  html: string;
  postMessage(message: unknown): PromiseLike<boolean> | boolean;
  onDidReceiveMessage(listener: (message: unknown) => void): DashboardDisposable;
}

export interface DashboardPanel {
  readonly webview: DashboardWebview;
  reveal(viewColumn?: unknown): void;
  onDidDispose(listener: () => void): DashboardDisposable;
}

export interface DashboardWindowApi {
  createWebviewPanel(
    viewType: string,
    title: string,
    viewColumn: unknown,
    options: {
      readonly enableScripts: boolean;
      readonly localResourceRoots: readonly unknown[];
      readonly retainContextWhenHidden: boolean;
    }
  ): DashboardPanel;
}

export interface DashboardCommandsApi {
  executeCommand?(commandId: string): PromiseLike<unknown> | unknown;
}

type ArchitectureStudioDashboardControllerOptions = {
  readonly commands: DashboardCommandsApi;
  readonly extensionUri: DashboardResourceUri;
  readonly getState?: () => DashboardState;
  readonly nonceFactory?: () => string;
  readonly output: StudioCommandOutput;
  readonly uri: DashboardUriApi;
  readonly viewColumn: unknown;
  readonly window: DashboardWindowApi;
};

export class ArchitectureStudioDashboardController {
  private activePanel?: DashboardPanel;
  private activePanelDisposables: DashboardDisposable[] = [];

  public constructor(private readonly options: ArchitectureStudioDashboardControllerOptions) {}

  public async show(): Promise<void> {
    if (this.activePanel) {
      this.activePanel.reveal(this.options.viewColumn);
      await this.postState(this.activePanel);
      return;
    }

    const mediaRoot = this.options.uri.joinPath(this.options.extensionUri, "media");
    const panel = this.options.window.createWebviewPanel(
      "architectureStudio.dashboard",
      "Architecture Studio",
      this.options.viewColumn,
      {
        enableScripts: true,
        localResourceRoots: [this.options.extensionUri, mediaRoot],
        retainContextWhenHidden: true
      }
    );
    const state = this.getState();

    this.activePanel = panel;
    panel.webview.html = renderDashboardHtml({
      extensionUri: this.options.extensionUri,
      nonce: this.createNonce(),
      state,
      uri: this.options.uri,
      webview: panel.webview
    });

    const disposeRegistration = panel.onDidDispose(() => {
      this.options.output.appendLine("[Architecture Studio] Dashboard panel disposed.");
      this.releasePanel(panel);
    });
    const messageRegistration = panel.webview.onDidReceiveMessage((message) => {
      void this.handleMessage(panel, message);
    });

    this.activePanelDisposables = [disposeRegistration, messageRegistration];
  }

  private createNonce(): string {
    return this.options.nonceFactory?.() ?? Math.random().toString(36).slice(2);
  }

  private getState(): DashboardState {
    return this.options.getState?.() ?? createDashboardState();
  }

  private async handleMessage(panel: DashboardPanel, message: unknown): Promise<void> {
    if (this.activePanel !== panel) {
      return;
    }

    if (!isDashboardWebviewMessage(message)) {
      this.options.output.appendLine("[Architecture Studio] Dashboard ignored unsupported webview message.");
      return;
    }

    switch (message.type) {
      case "dashboard.ready":
        this.options.output.appendLine("[Architecture Studio] Dashboard ready message received.");
        await this.postState(panel);
        break;
      case "dashboard.runCommand":
        this.options.output.appendLine(
          `[Architecture Studio] Dashboard message received: dashboard.runCommand -> ${message.commandId}`
        );
        await this.options.commands.executeCommand?.(message.commandId);
        break;
    }
  }

  private async postState(panel: DashboardPanel): Promise<void> {
    await panel.webview.postMessage(createDashboardStateMessage(this.getState()));
  }

  private releasePanel(panel: DashboardPanel): void {
    if (this.activePanel !== panel) {
      return;
    }

    this.activePanel = undefined;

    const disposables = this.activePanelDisposables;
    this.activePanelDisposables = [];

    for (const disposable of disposables) {
      disposable.dispose();
    }
  }
}
