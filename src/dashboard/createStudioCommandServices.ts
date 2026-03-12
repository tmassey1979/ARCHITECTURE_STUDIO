import type { StudioCommandOutput, StudioCommandServices } from "../commands/commandRuntime";
import { ArchitectureStudioDashboardController, type DashboardCommandsApi, type DashboardWindowApi } from "./dashboardController";
import type { DashboardResourceUri, DashboardUriApi } from "./dashboardHtml";
import {
  createArchitectureStudioCoreCli,
  type ArchitectureStudioCoreCli
} from "../core/architectureStudioCoreCli";

export type DashboardServiceFactoryContext = {
  readonly commands: DashboardCommandsApi;
  readonly coreCli?: ArchitectureStudioCoreCli;
  readonly extensionPath?: string;
  readonly extensionUri: DashboardResourceUri;
  readonly output: StudioCommandOutput;
  readonly uri: DashboardUriApi;
  readonly viewColumn: unknown;
  readonly window: DashboardWindowApi;
  readonly workspace: {
    getFirstWorkspaceFolderPath(): string | undefined;
  };
};

export function createStudioCommandServices({
  commands,
  coreCli: providedCoreCli,
  extensionPath,
  extensionUri,
  output,
  uri,
  viewColumn,
  window,
  workspace
}: DashboardServiceFactoryContext): StudioCommandServices {
  const coreCli =
    providedCoreCli
    ?? (extensionPath
      ? createArchitectureStudioCoreCli({
          extensionPath,
          output
        })
      : undefined);
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
    },
    getWorkspaceFolder() {
      return workspace.getFirstWorkspaceFolderPath();
    },
    ...(coreCli
      ? {
          runStandardsComposition(workspacePath: string) {
            return coreCli.composeStandards(workspacePath);
          },
          runRepositoryAnalysis(workspacePath: string) {
            return coreCli.analyzeRepository(workspacePath);
          },
          runArchitectureEvaluation(workspacePath: string) {
            return coreCli.evaluateArchitecture(workspacePath);
          },
          runComplianceEvaluation(workspacePath: string) {
            return coreCli.validateRegulations(workspacePath);
          },
          async getProjectSelection() {
            const workspacePath = workspace.getFirstWorkspaceFolderPath();
            return workspacePath ? coreCli.inferProjectSelection(workspacePath) : undefined;
          },
          runProjectGeneration(selection: Parameters<ArchitectureStudioCoreCli["generateProject"]>[0]) {
            return coreCli.generateProject(selection);
          },
          runReportGeneration(workspacePath: string) {
            return coreCli.generateReports(workspacePath);
          },
          getAiInstructionContext(workspacePath?: string) {
            return workspacePath ? coreCli.buildAiInstructionRequest(workspacePath) : undefined;
          },
          runAiInstructionGeneration(request: Parameters<ArchitectureStudioCoreCli["generateAiInstructions"]>[0]) {
            return coreCli.generateAiInstructions(request);
          }
        }
      : {})
  };
}
