import type { StudioCommandOutput, StudioCommandServices } from "../commands/commandRuntime";
import {
  createArchitectureStudioCoreCli,
  type ArchitectureStudioCoreCli
} from "../core/architectureStudioCoreCli";
import { createLiveDashboardState } from "./dashboardData";

export type DashboardServiceFactoryContext = {
  readonly coreCli?: ArchitectureStudioCoreCli;
  readonly extensionPath?: string;
  readonly output: StudioCommandOutput;
  readonly showDashboard?: () => Promise<void> | void;
  readonly workspace: {
    getFirstWorkspaceFolderPath(): string | undefined;
  };
};

export function createStudioCommandServices({
  coreCli: providedCoreCli,
  extensionPath,
  output,
  showDashboard,
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
  let services: StudioCommandServices;

  services = {
    ...(showDashboard ? { showDashboard } : {}),
    async getDashboardState() {
      return createLiveDashboardState(services);
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

  return services;
}
