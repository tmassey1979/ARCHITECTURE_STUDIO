import type { StudioCommandHandler } from "../commandRuntime";

export function createPlaceholderCommandHandler(): StudioCommandHandler {
  return async ({ route, host, output }) => {
    output.appendLine(`[Architecture Studio] Routed ${route.id} -> ${route.handlerModule}`);
    await host.showInformationMessage(`${route.title} is scaffolded and ready for implementation.`);
  };
}
