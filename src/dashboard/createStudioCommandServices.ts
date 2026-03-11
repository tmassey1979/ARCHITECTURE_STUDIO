import type { StudioCommandOutput, StudioCommandServices } from "../commands/commandRuntime";
import { ArchitectureStudioDashboardController, type DashboardCommandsApi, type DashboardWindowApi } from "./dashboardController";
import type { DashboardResourceUri, DashboardUriApi } from "./dashboardHtml";

export type DashboardServiceFactoryContext = {
  readonly commands: DashboardCommandsApi;
  readonly extensionUri: DashboardResourceUri;
  readonly output: StudioCommandOutput;
  readonly uri: DashboardUriApi;
  readonly viewColumn: unknown;
  readonly window: DashboardWindowApi;
};

export function createStudioCommandServices({
  commands,
  extensionUri,
  output,
  uri,
  viewColumn,
  window
}: DashboardServiceFactoryContext): StudioCommandServices {
  const dashboard = new ArchitectureStudioDashboardController({
    commands,
    extensionUri,
    output,
    uri,
    viewColumn,
    window
  });

  return {
    async showDashboard() {
      await dashboard.show();
    }
  };
}
