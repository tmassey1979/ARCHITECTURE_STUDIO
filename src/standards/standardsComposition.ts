import type { StandardCategory, StandardDefinition } from "../contracts/sharedContracts";

export const standardsConsumerWorkflows = [
  "Dashboard",
  "ProjectGenerator",
  "ReportGenerator",
  "AiInstructions"
] as const;

export type StandardsConsumerWorkflow = (typeof standardsConsumerWorkflows)[number];

export type StandardsSourceMetadata = {
  readonly packageId: string;
  readonly packageVersion: string;
  readonly sourcePath: string;
  readonly sourceTitle: string;
};

export type ComposedStandard = {
  readonly definition: StandardDefinition;
  readonly source: StandardsSourceMetadata;
  readonly selectionReasons: readonly string[];
};

export type StandardsConsumerHint = {
  readonly workflow: StandardsConsumerWorkflow;
  readonly usage: string;
};

export type StandardsProjectSelection = {
  readonly frontend: string;
  readonly backend: string;
  readonly architecturePattern: string;
  readonly ciCd: readonly string[];
  readonly infrastructure: readonly string[];
  readonly additionalSelections: readonly string[];
};

export type RepositoryCharacteristics = {
  readonly detectedTechnologies: readonly string[];
  readonly detectedTags: readonly string[];
  readonly detectedCategories: readonly StandardCategory[];
};

export type StandardsCompositionRequest = {
  readonly projectSelection?: StandardsProjectSelection;
  readonly repositoryCharacteristics?: RepositoryCharacteristics;
};

export type ComposedStandardsResult = {
  readonly standards: readonly ComposedStandard[];
  readonly consumerHints: readonly StandardsConsumerHint[];
};
