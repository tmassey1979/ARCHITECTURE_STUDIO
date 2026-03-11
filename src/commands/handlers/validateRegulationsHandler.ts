import type { StudioCommandHandler } from "../commandRuntime";

const noWorkspaceMessage =
  "Architecture Studio requires an open workspace to validate regulations.";

export default (async ({ host, output, services }) => {
  const workspacePath = await services.getWorkspaceFolder?.();

  if (!workspacePath) {
    output.appendLine(`[Architecture Studio] ${noWorkspaceMessage}`);
    await host.showErrorMessage(noWorkspaceMessage);
    return;
  }

  output.appendLine(`[Architecture Studio] Target workspace: ${workspacePath}`);

  const evaluation = await services.runComplianceEvaluation?.(workspacePath);
  const summaries = evaluation?.summaries ?? [];
  const findings = evaluation?.findings ?? [];

  for (const summary of summaries) {
    output.appendLine(`[Architecture Studio] ${summary.regulationTitle} ${summary.scorePercentage}%`);
  }

  output.appendLine(`[Architecture Studio] Findings: ${findings.length}`);

  await host.showInformationMessage(
    `Evaluated ${summaries.length} applicable regulations and ${findings.length} findings in ${workspacePath}.`
  );
}) satisfies StudioCommandHandler;
