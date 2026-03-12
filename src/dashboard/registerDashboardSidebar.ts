import type { ExtensionContext } from "vscode";

import type { DashboardSidebarViewProvider } from "./dashboardSidebarProvider";

type DisposableLike = { dispose(): void };

export interface DashboardSidebarWindowApi {
  registerWebviewViewProvider(
    viewId: string,
    provider: DashboardSidebarViewProvider
  ): DisposableLike;
}

export const architectureStudioDashboardViewId = "architectureStudio.dashboardView";
export const architectureStudioDashboardViewFocusCommandId = `${architectureStudioDashboardViewId}.focus`;

export function registerArchitectureStudioDashboardSidebar(
  context: ExtensionContext,
  windowApi: DashboardSidebarWindowApi,
  provider: DashboardSidebarViewProvider
): void {
  context.subscriptions.push(windowApi.registerWebviewViewProvider(architectureStudioDashboardViewId, provider));
}
