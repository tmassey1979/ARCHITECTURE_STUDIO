import * as vscode from "vscode";

import type { StudioCommandServices } from "./commands/commandRuntime";
import {
  architectureStudioOutputChannelName,
  registerArchitectureStudioCommands
} from "./commands/registerCommands";
import { createStudioCommandServices } from "./dashboard/createStudioCommandServices";
import { ArchitectureStudioDashboardSidebarProvider } from "./dashboard/dashboardSidebarProvider";
import { createDashboardState } from "./dashboard/dashboardState";
import {
  architectureStudioDashboardViewFocusCommandId,
  registerArchitectureStudioDashboardSidebar
} from "./dashboard/registerDashboardSidebar";

export function activate(context: vscode.ExtensionContext): void {
  const outputChannel = vscode.window.createOutputChannel(architectureStudioOutputChannelName);
  let services: StudioCommandServices;
  const dashboardSidebarProvider = new ArchitectureStudioDashboardSidebarProvider({
    commands: {
      executeCommand(commandId) {
        return vscode.commands.executeCommand(commandId);
      }
    },
    extensionUri: context.extensionUri,
    focusCommandId: architectureStudioDashboardViewFocusCommandId,
    getState() {
      return services.getDashboardState?.() ?? createDashboardState();
    },
    output: outputChannel,
    uri: vscode.Uri
  });

  services = createStudioCommandServices({
    extensionPath: context.extensionPath,
    output: outputChannel,
    showDashboard() {
      return dashboardSidebarProvider.show();
    },
    workspace: {
      getFirstWorkspaceFolderPath() {
        return vscode.workspace.workspaceFolders?.[0]?.uri.fsPath;
      }
    }
  });

  registerArchitectureStudioDashboardSidebar(
    context,
    {
      registerWebviewViewProvider(viewId, provider) {
        return vscode.window.registerWebviewViewProvider(viewId, provider as vscode.WebviewViewProvider);
      }
    },
    dashboardSidebarProvider
  );

  registerArchitectureStudioCommands(
    context,
    {
      commands: vscode.commands,
      window: vscode.window
    },
    {
      outputChannel,
      services
    }
  );
}

export function deactivate(): void {
  // No long-lived resources yet.
}
