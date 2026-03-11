import type { RepositoryAnalysisResult } from "../analysis/repositoryAnalysis";
import type { ComplianceSummary, FindingDefinition } from "../contracts/sharedContracts";

export type ComplianceEvaluationRequest = {
  readonly systemCharacteristics: readonly string[];
  readonly repositoryAnalysis: RepositoryAnalysisResult;
  readonly implementedControlIds: readonly string[];
};

export type ComplianceEvaluationResult = {
  readonly summaries: readonly ComplianceSummary[];
  readonly findings: readonly FindingDefinition[];
};
