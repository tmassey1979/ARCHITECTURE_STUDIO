import type { StudioCommandHandler } from "../commandRuntime";

const noWorkspaceMessage =
  "Architecture Studio requires an open workspace to generate architecture guidance.";

export default (async ({ host, output, services }) => {
  const workspacePath = await services.getWorkspaceFolder?.();

  if (!workspacePath) {
    output.appendLine(`[Architecture Studio] ${noWorkspaceMessage}`);
    await host.showErrorMessage(noWorkspaceMessage);
    return;
  }

  output.appendLine(`[Architecture Studio] Target workspace: ${workspacePath}`);

  const evaluation = await services.runArchitectureEvaluation?.(workspacePath);
  const selectedNodes = evaluation?.technologyEvaluation.selectedNodes ?? [];
  const recommendations = evaluation?.technologyEvaluation.recommendations ?? [];
  const findings = evaluation?.findings ?? [];

  output.appendLine(`[Architecture Studio] Selected nodes: ${selectedNodes.length}`);
  output.appendLine(`[Architecture Studio] Recommendations: ${recommendations.length}`);
  output.appendLine(`[Architecture Studio] Findings: ${findings.length}`);

  await host.showInformationMessage(
    `Evaluated ${selectedNodes.length} architecture nodes, ${recommendations.length} recommendations, and ${findings.length} findings in ${workspacePath}.`
  );
}) satisfies StudioCommandHandler;
