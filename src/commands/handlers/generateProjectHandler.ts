import type { StudioCommandHandler } from "../commandRuntime";

const noSelectionMessage =
  "Architecture Studio requires a project generation selection before it can scaffold artifacts.";

export default (async ({ host, output, services }) => {
  const selection = await services.getProjectSelection?.();

  if (!selection) {
    output.appendLine(`[Architecture Studio] ${noSelectionMessage}`);
    await host.showErrorMessage(noSelectionMessage);
    return;
  }

  output.appendLine(
    `[Architecture Studio] Selection: ${selection.frontend} / ${selection.backend} / ${selection.architecturePattern}`
  );

  const generationResult = await services.runProjectGeneration?.(selection);
  const templateIds = generationResult?.templateIds ?? [];
  const generatedArtifacts = generationResult?.generatedArtifacts ?? [];

  output.appendLine(`[Architecture Studio] Generated artifacts: ${generatedArtifacts.length}`);
  output.appendLine(`[Architecture Studio] Template set: ${templateIds.join(", ")}`);

  await host.showInformationMessage(
    `Generated ${generatedArtifacts.length} artifacts for ${selection.architecturePattern}.`
  );
}) satisfies StudioCommandHandler;
