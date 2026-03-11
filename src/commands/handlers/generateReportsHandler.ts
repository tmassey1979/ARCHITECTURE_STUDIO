import type { StudioCommandHandler } from "../commandRuntime";

const noWorkspaceMessage =
  "Architecture Studio requires an open workspace to generate reports.";

export default (async ({ host, output, services }) => {
  const workspacePath = await services.getWorkspaceFolder?.();

  if (!workspacePath) {
    output.appendLine(`[Architecture Studio] ${noWorkspaceMessage}`);
    await host.showErrorMessage(noWorkspaceMessage);
    return;
  }

  output.appendLine(`[Architecture Studio] Target workspace: ${workspacePath}`);

  const reportResult = await services.runReportGeneration?.(workspacePath);
  const reportArtifacts = reportResult?.reportArtifacts ?? [];

  output.appendLine(`[Architecture Studio] Report artifacts: ${reportArtifacts.length}`);
  output.appendLine(`[Architecture Studio] PDF fallback: ${reportResult?.pdfFallbackUsed ? "enabled" : "disabled"}`);

  await host.showInformationMessage(
    `Generated ${reportArtifacts.length} report artifacts for ${workspacePath}.`
  );
}) satisfies StudioCommandHandler;
