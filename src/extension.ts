import type { ExtensionContext } from "vscode";

import { registerArchitectureStudioCommands } from "./commands/registerCommands";

export function activate(context: ExtensionContext): void {
  registerArchitectureStudioCommands(context);
}

export function deactivate(): void {
  // No long-lived resources yet.
}
