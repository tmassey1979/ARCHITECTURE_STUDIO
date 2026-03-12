import type {
  ComplianceSummary,
  FindingDefinition,
  GeneratedArtifact,
  GeneratedArtifactKind,
  ProjectSelectionProfile,
  StandardDefinition
} from "../contracts/sharedContracts";

export const aiInstructionTargetKinds = ["GeneratedProject", "AnalyzedRepository"] as const;

export type AiInstructionTargetKind = (typeof aiInstructionTargetKinds)[number];

export type AiInstructionGenerationRequest = {
  readonly projectName: string;
  readonly targetKind: AiInstructionTargetKind;
  readonly projectSelection: ProjectSelectionProfile;
  readonly standards: readonly StandardDefinition[];
  readonly complianceSummaries: readonly ComplianceSummary[];
  readonly findings: readonly FindingDefinition[];
};

export type AiInstructionGeneratedFile = {
  readonly relativePath: string;
  readonly title: string;
  readonly kind: GeneratedArtifactKind;
  readonly content: string;
};

export type AiInstructionGenerationResult = {
  readonly generatedArtifacts: readonly GeneratedArtifact[];
  readonly files: readonly AiInstructionGeneratedFile[];
};
