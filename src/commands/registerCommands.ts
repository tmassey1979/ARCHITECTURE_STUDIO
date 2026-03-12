import type { ExtensionContext } from "vscode";

import {
  createCommandExecutor,
  type StudioCommandHost,
  type StudioCommandOutput,
  type StudioCommandServices,
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
  executeCommand?(commandId: string, ...commandArguments: unknown[]): PromiseLike<unknown> | unknown;
}

export interface StudioVscodeApi {
  readonly commands: StudioCommandsApi;
  readonly window: StudioWindowApi;
}

export const architectureStudioOutputChannelName = "Architecture Studio";

export interface RegisterArchitectureStudioCommandsOptions {
  readonly services?: StudioCommandServices;
  readonly createServices?: (outputChannel: StudioOutputChannel) => StudioCommandServices;
  readonly outputChannel?: StudioOutputChannel;
}

export function registerArchitectureStudioCommands(
  context: ExtensionContext,
  vscodeApi: StudioVscodeApi,
  options: RegisterArchitectureStudioCommandsOptions = {}
): void {
  const outputChannel = options.outputChannel ?? vscodeApi.window.createOutputChannel(architectureStudioOutputChannelName);
  const services = options.services ?? options.createServices?.(outputChannel) ?? {};
  context.subscriptions.push(outputChannel);

  for (const route of studioCommandRoutes) {
    const registration = vscodeApi.commands.registerCommand(
      route.id,
      createCommandExecutor(route, vscodeApi.window, outputChannel, services)
    );

    context.subscriptions.push(registration);
  }
}
