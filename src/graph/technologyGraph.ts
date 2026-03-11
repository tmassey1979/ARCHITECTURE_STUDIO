import type {
  FindingDefinition,
  GraphEdgeDefinition,
  GraphNodeDefinition,
  GraphRelationship
} from "../contracts/sharedContracts";

export type TechnologyGraph = {
  readonly nodes: readonly GraphNodeDefinition[];
  readonly edges: readonly GraphEdgeDefinition[];
};

export type TechnologyStackSelection = {
  readonly selectedNodeIds: readonly string[];
};

export type TechnologyMissingRequirement = {
  readonly sourceNodeId: string;
  readonly requiredNodeId: string;
};

export type TechnologyConflict = {
  readonly leftNodeId: string;
  readonly rightNodeId: string;
};

export type TechnologyRecommendation = {
  readonly sourceNodeId: string;
  readonly nodeId: string;
  readonly relationship: GraphRelationship;
};

export type TechnologyEvaluationResult = {
  readonly selectedNodes: readonly GraphNodeDefinition[];
  readonly missingRequirements: readonly TechnologyMissingRequirement[];
  readonly conflicts: readonly TechnologyConflict[];
  readonly recommendations: readonly TechnologyRecommendation[];
};

export type ArchitectureValidationRequest = {
  readonly domainToInfrastructureReferences: readonly string[];
  readonly uiBusinessLogicFiles: readonly string[];
  readonly controllerDatabaseAccesses: readonly string[];
  readonly authenticationConfigured: boolean;
};

export type ArchitectureValidationResult = {
  readonly findings: readonly FindingDefinition[];
};
