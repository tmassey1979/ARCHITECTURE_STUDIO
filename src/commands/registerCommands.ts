import * as vscode from "vscode";

import { architectureStudioCommands } from "./commandDefinitions";

export function registerArchitectureStudioCommands(context: vscode.ExtensionContext): void {
  const outputChannel = vscode.window.createOutputChannel("Architecture Studio");
  context.subscriptions.push(outputChannel);

  for (const command of architectureStudioCommands) {
    const registration = vscode.commands.registerCommand(command.id, async () => {
      outputChannel.appendLine(`Command invoked: ${command.id}`);
      await vscode.window.showInformationMessage(`${command.title} is scaffolded and ready for implementation.`);
    });

    context.subscriptions.push(registration);
  }
}
