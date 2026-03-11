import type { ExtensionContext } from "vscode";

import {
  createCommandExecutor,
  type StudioCommandHost,
  type StudioCommandOutput,
  studioCommandRoutes
} from "./commandRuntime";

type DisposableLike = { dispose(): void };

export interface StudioOutputChannel extends StudioCommandOutput, DisposableLike {}

export interface StudioWindowApi extends StudioCommandHost {
  createOutputChannel(name: string): StudioOutputChannel;
}

export interface StudioCommandsApi {
  registerCommand(
    id: string,
    handler: (...commandArguments: unknown[]) => Promise<void>
  ): DisposableLike;
}

export interface StudioVscodeApi {
  readonly commands: StudioCommandsApi;
  readonly window: StudioWindowApi;
}

export const architectureStudioOutputChannelName = "Architecture Studio";

export function registerArchitectureStudioCommands(
  context: ExtensionContext,
  vscodeApi: StudioVscodeApi
): void {
  const outputChannel = vscodeApi.window.createOutputChannel(architectureStudioOutputChannelName);
  context.subscriptions.push(outputChannel);

  for (const route of studioCommandRoutes) {
    const registration = vscodeApi.commands.registerCommand(
      route.id,
      createCommandExecutor(route, vscodeApi.window, outputChannel)
    );

    context.subscriptions.push(registration);
  }
}
