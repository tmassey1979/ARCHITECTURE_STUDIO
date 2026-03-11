import type { StudioCommandHandler } from "../commandRuntime";

const noWorkspaceMessage =
  "Architecture Studio requires an open workspace to analyze a repository.";

export default (async ({ host, output, services }) => {
  const workspacePath = await services.getWorkspaceFolder?.();

  if (!workspacePath) {
    output.appendLine(`[Architecture Studio] ${noWorkspaceMessage}`);
    await host.showErrorMessage(noWorkspaceMessage);
    return;
  }

  output.appendLine(`[Architecture Studio] Target workspace: ${workspacePath}`);

  const analysisResult = await services.runRepositoryAnalysis?.(workspacePath);
  const signalCount = analysisResult?.signals.length ?? 0;
  const sensitiveDataCount = analysisResult?.sensitiveData.length ?? 0;

  output.appendLine(`[Architecture Studio] Signals: ${signalCount}`);
  output.appendLine(`[Architecture Studio] Sensitive data classifications: ${sensitiveDataCount}`);

  await host.showInformationMessage(
    `Analyzed ${signalCount} repository signals and ${sensitiveDataCount} sensitive data classifications in ${workspacePath}.`
  );
}) satisfies StudioCommandHandler;
