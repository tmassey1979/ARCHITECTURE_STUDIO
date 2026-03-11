import { architectureStudioCommands, type ArchitectureStudioCommand } from "./commandDefinitions";

export interface StudioCommandHost {
  showInformationMessage(message: string): PromiseLike<unknown> | unknown;
  showErrorMessage(message: string): PromiseLike<unknown> | unknown;
}

export interface StudioCommandOutput {
  appendLine(line: string): void;
}

export interface StudioCommandExecutionContext {
  readonly route: StudioCommandRoute;
  readonly host: StudioCommandHost;
  readonly output: StudioCommandOutput;
  readonly arguments: readonly unknown[];
}

export type StudioCommandHandler = (
  context: StudioCommandExecutionContext
) => Promise<void> | void;

export type StudioCommandRoute = ArchitectureStudioCommand & {
  readonly loadHandler?: () => Promise<StudioCommandHandler>;
};

export const studioCommandRoutes: readonly StudioCommandRoute[] = architectureStudioCommands;

async function loadCommandHandler(route: StudioCommandRoute): Promise<StudioCommandHandler> {
  if (route.loadHandler) {
    return route.loadHandler();
  }

  const importedModule = (await import(route.handlerModule)) as {
    default?: unknown;
    handleCommand?: unknown;
  };
  const handler = importedModule.default ?? importedModule.handleCommand;

  if (typeof handler !== "function") {
    throw new Error(`Command handler module did not export a function: ${route.handlerModule}`);
  }

  return handler as StudioCommandHandler;
}

function normalizeError(error: unknown): string {
  if (error instanceof Error) {
    return error.message;
  }

  return String(error);
}

export function createCommandExecutor(
  route: StudioCommandRoute,
  host: StudioCommandHost,
  output: StudioCommandOutput
): (...commandArguments: unknown[]) => Promise<void> {
  return async (...commandArguments: unknown[]) => {
    output.appendLine(`[Architecture Studio] Command invoked: ${route.id}`);

    try {
      const handler = await loadCommandHandler(route);

      await handler({
        route,
        host,
        output,
        arguments: commandArguments
      });
    } catch (error) {
      const message = normalizeError(error);

      output.appendLine(`[Architecture Studio] Command failed: ${route.id} - ${message}`);
      await host.showErrorMessage(`Architecture Studio command failed (${route.id}): ${message}`);
    }
  };
}
