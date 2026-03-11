import type { GeneratedArtifact, GeneratedArtifactKind, ProjectSelectionProfile } from "../contracts/sharedContracts";

export type ProjectGeneratedFile = {
  readonly relativePath: string;
  readonly title: string;
  readonly kind: GeneratedArtifactKind;
  readonly content: string;
};

export type ProjectGenerationResult = {
  readonly templateIds: readonly string[];
  readonly generatedArtifacts: readonly GeneratedArtifact[];
  readonly files: readonly ProjectGeneratedFile[];
};

export type ProjectGenerationRequest = {
  readonly selection: ProjectSelectionProfile;
};
