import type { StudioCommandOutput } from "../commands/commandRuntime";
import { createDashboardState, createDashboardStateMessage, isDashboardWebviewMessage, type DashboardState } from "./dashboardState";
import { renderDashboardHtml, type DashboardHtmlWebview, type DashboardResourceUri, type DashboardUriApi } from "./dashboardHtml";

export interface DashboardSidebarDisposable {
  dispose(): void;
}

export interface DashboardSidebarWebview extends DashboardHtmlWebview {
  html: string;
  options?: {
    readonly enableScripts: boolean;
    readonly localResourceRoots: readonly unknown[];
    readonly retainContextWhenHidden?: boolean;
  };
  postMessage(message: unknown): PromiseLike<boolean> | boolean;
  onDidReceiveMessage(listener: (message: unknown) => void): DashboardSidebarDisposable;
}

export interface DashboardSidebarView {
  readonly viewType: string;
  readonly visible: boolean;
  readonly webview: DashboardSidebarWebview;
  show(preserveFocus?: boolean): void;
  onDidDispose(listener: () => void): DashboardSidebarDisposable;
  onDidChangeVisibility(listener: () => void): DashboardSidebarDisposable;
}

export interface DashboardSidebarCommandsApi {
  executeCommand?(commandId: string): PromiseLike<unknown> | unknown;
}

export interface DashboardSidebarViewProvider {
  resolveWebviewView(view: DashboardSidebarView, context?: unknown, token?: unknown): Promise<void> | void;
}

type ArchitectureStudioDashboardSidebarProviderOptions = {
  readonly commands: DashboardSidebarCommandsApi;
  readonly extensionUri: DashboardResourceUri;
  readonly focusCommandId: string;
  readonly getState?: () => Promise<DashboardState> | DashboardState;
  readonly nonceFactory?: () => string;
  readonly output: StudioCommandOutput;
  readonly uri: DashboardUriApi;
};

export class ArchitectureStudioDashboardSidebarProvider implements DashboardSidebarViewProvider {
  private activeView?: DashboardSidebarView;
  private activeViewDisposables: DashboardSidebarDisposable[] = [];
  private stateVersion = 0;

  public constructor(private readonly options: ArchitectureStudioDashboardSidebarProviderOptions) {}

  public async show(): Promise<void> {
    if (this.activeView) {
      this.activeView.show(false);
      await this.postState(this.activeView);
      return;
    }

    await this.options.commands.executeCommand?.(this.options.focusCommandId);
  }

  public async resolveWebviewView(view: DashboardSidebarView): Promise<void> {
    if (this.activeView && this.activeView !== view) {
      this.releaseView(this.activeView);
    }

    this.activeView = view;

    const mediaRoot = this.options.uri.joinPath(this.options.extensionUri, "media");
    view.webview.options = {
      enableScripts: true,
      localResourceRoots: [this.options.extensionUri, mediaRoot],
      retainContextWhenHidden: true
    };

    const state = await this.getState();
    view.webview.html = renderDashboardHtml({
      extensionUri: this.options.extensionUri,
      nonce: this.createNonce(),
      state,
      uri: this.options.uri,
      webview: view.webview
    });

    const disposeRegistration = view.onDidDispose(() => {
      this.options.output.appendLine("[Architecture Studio] Dashboard sidebar disposed.");
      this.releaseView(view);
    });
    const visibilityRegistration = view.onDidChangeVisibility(() => {
      if (this.activeView === view && view.visible) {
        void this.postState(view);
      }
    });
    const messageRegistration = view.webview.onDidReceiveMessage((message) => {
      void this.handleMessage(view, message);
    });

    this.activeViewDisposables = [disposeRegistration, visibilityRegistration, messageRegistration];
  }

  private createNonce(): string {
    return this.options.nonceFactory?.() ?? Math.random().toString(36).slice(2);
  }

  private async getState(): Promise<DashboardState> {
    return (await this.options.getState?.()) ?? createDashboardState();
  }

  private async handleMessage(view: DashboardSidebarView, message: unknown): Promise<void> {
    if (this.activeView !== view) {
      return;
    }

    if (!isDashboardWebviewMessage(message)) {
      this.options.output.appendLine("[Architecture Studio] Dashboard sidebar ignored unsupported webview message.");
      return;
    }

    switch (message.type) {
      case "dashboard.ready":
        this.options.output.appendLine("[Architecture Studio] Dashboard sidebar ready message received.");
        await this.postState(view);
        break;
      case "dashboard.runCommand":
        this.options.output.appendLine(
          `[Architecture Studio] Dashboard sidebar message received: dashboard.runCommand -> ${message.commandId}`
        );
        await this.options.commands.executeCommand?.(message.commandId);
        await this.postState(view);
        break;
    }
  }

  private async postState(view: DashboardSidebarView): Promise<void> {
    const requestedVersion = ++this.stateVersion;
    const state = await this.getState();

    if (this.activeView !== view || requestedVersion !== this.stateVersion) {
      return;
    }

    await view.webview.postMessage(createDashboardStateMessage(state));
  }

  private releaseView(view: DashboardSidebarView): void {
    if (this.activeView !== view) {
      return;
    }

    this.activeView = undefined;

    const disposables = this.activeViewDisposables;
    this.activeViewDisposables = [];

    for (const disposable of disposables) {
      disposable.dispose();
    }
  }
}
