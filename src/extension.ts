import * as vscode from "vscode";

import { createStudioCommandServices } from "./dashboard/createStudioCommandServices";
import { registerArchitectureStudioCommands } from "./commands/registerCommands";

export function activate(context: vscode.ExtensionContext): void {
  registerArchitectureStudioCommands(
    context,
    {
      commands: vscode.commands,
      window: vscode.window
    },
    {
      createServices: (outputChannel) =>
        createStudioCommandServices({
          commands: {
            executeCommand(commandId) {
              return vscode.commands.executeCommand(commandId);
            }
          },
          extensionUri: context.extensionUri,
          output: outputChannel,
          uri: vscode.Uri,
          viewColumn: vscode.ViewColumn.One,
          window: {
            createWebviewPanel(viewType, title, viewColumn, options) {
              return vscode.window.createWebviewPanel(
                viewType,
                title,
                viewColumn as vscode.ViewColumn,
                options as vscode.WebviewPanelOptions & vscode.WebviewOptions
              );
            }
          }
        })
    }
  );
}

export function deactivate(): void {
  // No long-lived resources yet.
}
