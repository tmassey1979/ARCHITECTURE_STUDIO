import * as vscode from "vscode";

import { registerArchitectureStudioCommands } from "./commands/registerCommands";

export function activate(context: vscode.ExtensionContext): void {
  registerArchitectureStudioCommands(context, {
    commands: vscode.commands,
    window: vscode.window
  });
}

export function deactivate(): void {
  // No long-lived resources yet.
}
