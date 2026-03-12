import type { StudioCommandHandler } from "../commandRuntime";

const noWorkspaceMessage =
  "Architecture Studio requires an open workspace to compose standards.";

export default (async ({ host, output, services }) => {
  const workspacePath = await services.getWorkspaceFolder?.();

  if (!workspacePath) {
    output.appendLine(`[Architecture Studio] ${noWorkspaceMessage}`);
    await host.showErrorMessage(noWorkspaceMessage);
    return;
  }

  output.appendLine(`[Architecture Studio] Target workspace: ${workspacePath}`);

  const compositionResult = await services.runStandardsComposition?.(workspacePath);
  const standards = compositionResult?.standards ?? [];
  const topStandard = standards[0]?.definition.title ?? "None";

  output.appendLine(`[Architecture Studio] Standards: ${standards.length}`);
  output.appendLine(`[Architecture Studio] Top standard: ${topStandard}`);

  await host.showInformationMessage(`Composed ${standards.length} standards for ${workspacePath}.`);
}) satisfies StudioCommandHandler;
