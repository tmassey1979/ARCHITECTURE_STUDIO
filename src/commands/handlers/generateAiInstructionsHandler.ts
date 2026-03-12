import type { StudioCommandHandler } from "../commandRuntime";

const noContextMessage =
  "Architecture Studio requires AI instruction context before it can generate AGENTS.md guidance.";

export default (async ({ host, output, services }) => {
  const workspacePath = await services.getWorkspaceFolder?.();
  const context = await services.getAiInstructionContext?.(workspacePath);

  if (!context) {
    output.appendLine(`[Architecture Studio] ${noContextMessage}`);
    await host.showErrorMessage(noContextMessage);
    return;
  }

  if (workspacePath) {
    output.appendLine(`[Architecture Studio] Target workspace: ${workspacePath}`);
  }

  output.appendLine(`[Architecture Studio] Target kind: ${context.targetKind}`);

  const generationResult = await services.runAiInstructionGeneration?.(context);
  const generatedArtifacts = generationResult?.generatedArtifacts ?? [];
  const agentsGenerated = generatedArtifacts.some((artifact) => artifact.relativePath === "AGENTS.md");

  output.appendLine(`[Architecture Studio] Generated artifacts: ${generatedArtifacts.length}`);
  output.appendLine(`[Architecture Studio] AGENTS.md: ${agentsGenerated ? "generated" : "missing"}`);

  await host.showInformationMessage(
    `Generated ${generatedArtifacts.length} AI instruction artifacts for ${context.projectName}.`
  );
}) satisfies StudioCommandHandler;
