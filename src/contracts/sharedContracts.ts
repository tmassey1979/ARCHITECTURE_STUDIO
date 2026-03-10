export const severityLevels = ["Critical", "High", "Medium", "Low"] as const;
export const riskLevels = ["Critical", "High", "Medium", "Low"] as const;

export type SeverityLevel = (typeof severityLevels)[number];
export type RiskLevel = (typeof riskLevels)[number];

export type StandardCategory =
  | "Principles"
  | "Architecture"
  | "Frontend"
  | "Backend"
  | "DevOps"
  | "Testing"
  | "Security"
  | "Observability"
  | "Process";

export type RegulationCategory = "Privacy" | "Healthcare" | "Financial" | "Security" | "Communications";

export type GraphNodeCategory =
  | "Technology"
  | "Framework"
  | "ArchitecturePattern"
  | "Regulation"
  | "Control";

export type GraphRelationship = "Requires" | "Conflicts" | "PairsWith" | "RecommendedWith";
export type ArtifactFormat = "Markdown" | "Pdf" | "Json" | "Sarif";
export type GeneratedArtifactKind =
  | "ProjectScaffold"
  | "Pipeline"
  | "Infrastructure"
  | "Documentation"
  | "Report"
  | "AiInstructions";

export type StandardDefinition = {
  readonly id: string;
  readonly title: string;
  readonly category: StandardCategory;
  readonly summary: string;
  readonly tags: readonly string[];
};

export type RegulationDefinition = {
  readonly id: string;
  readonly category: RegulationCategory;
  readonly jurisdiction: string;
  readonly requiredControls: readonly string[];
  readonly dataTypes: readonly string[];
};

export type ControlDefinition = {
  readonly id: string;
  readonly title: string;
  readonly summary: string;
};

export type GraphNodeDefinition = {
  readonly id: string;
  readonly label: string;
  readonly category: GraphNodeCategory;
};

export type GraphEdgeDefinition = {
  readonly sourceId: string;
  readonly targetId: string;
  readonly relationship: GraphRelationship;
};

export type RemediationDefinition = {
  readonly title: string;
  readonly summary: string;
  readonly steps?: readonly string[];
};

export type FindingDefinition = {
  readonly id: string;
  readonly title: string;
  readonly summary: string;
  readonly severity: SeverityLevel;
  readonly risk: RiskLevel;
  readonly remediation: RemediationDefinition;
  readonly evidence?: readonly string[];
};

export type ReportArtifact = {
  readonly id: string;
  readonly title: string;
  readonly format: ArtifactFormat;
  readonly relativePath: string;
};

export type GeneratedArtifact = {
  readonly id: string;
  readonly title: string;
  readonly kind: GeneratedArtifactKind;
  readonly relativePath: string;
};

export type ProjectSelectionProfile = {
  readonly frontend: string;
  readonly backend: string;
  readonly architecturePattern: string;
  readonly ciCd: readonly string[];
  readonly infrastructure: readonly string[];
  readonly complianceTargets: readonly string[];
};

export type SharedContractPayload = {
  readonly standards: readonly StandardDefinition[];
  readonly regulations: readonly RegulationDefinition[];
  readonly controls: readonly ControlDefinition[];
  readonly graphNodes: readonly GraphNodeDefinition[];
  readonly graphEdges: readonly GraphEdgeDefinition[];
  readonly findings: readonly FindingDefinition[];
  readonly reports: readonly ReportArtifact[];
  readonly generatedArtifacts: readonly GeneratedArtifact[];
  readonly projectSelection: ProjectSelectionProfile;
};

export function isSeverityLevel(value: string): value is SeverityLevel {
  return severityLevels.includes(value as SeverityLevel);
}

export function isRiskLevel(value: string): value is RiskLevel {
  return riskLevels.includes(value as RiskLevel);
}
