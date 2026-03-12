import path from "node:path";

import type { RepositoryAnalysisResult, SensitiveDataClassification } from "../analysis/repositoryAnalysis";
import type { StudioCommandServices } from "../commands/commandRuntime";
import type {
  ComplianceSummary,
  FindingDefinition,
  GeneratedArtifact,
  GraphEdgeDefinition,
  ProjectSelectionProfile,
  SharedContractPayload
} from "../contracts/sharedContracts";
import type { WorkspaceArchitectureEvaluationResult } from "../graph/technologyGraph";
import { createDashboardState, createEmptySharedContractPayload, type DashboardState } from "./dashboardState";

type DashboardDataServices = Pick<
  StudioCommandServices,
  | "getAiInstructionContext"
  | "getProjectSelection"
  | "getWorkspaceFolder"
  | "runAiInstructionGeneration"
  | "runArchitectureEvaluation"
  | "runComplianceEvaluation"
  | "runProjectGeneration"
  | "runReportGeneration"
  | "runRepositoryAnalysis"
  | "runStandardsComposition"
>;

const emptyRepositoryAnalysis: RepositoryAnalysisResult = {
  signals: [],
  sensitiveData: []
};

const severityOrder: Record<FindingDefinition["severity"], number> = {
  Critical: 0,
  High: 1,
  Medium: 2,
  Low: 3
};

function getWorkspaceLabel(workspacePath: string): string {
  return path.basename(workspacePath.replace(/\\/g, "/"));
}

function sortById<T extends { readonly id: string }>(values: readonly T[]): readonly T[] {
  return [...values].sort((left, right) => left.id.localeCompare(right.id, "en", { sensitivity: "base" }));
}

function sortGeneratedArtifacts(artifacts: readonly GeneratedArtifact[]): readonly GeneratedArtifact[] {
  return [...artifacts].sort((left, right) => left.relativePath.localeCompare(right.relativePath, "en", { sensitivity: "base" }));
}

function sortComplianceSummaries(summaries: readonly ComplianceSummary[]): readonly ComplianceSummary[] {
  return [...summaries].sort((left, right) =>
    left.regulationId.localeCompare(right.regulationId, "en", { sensitivity: "base" })
  );
}

function sortFindings(findings: readonly FindingDefinition[]): readonly FindingDefinition[] {
  return [...findings].sort((left, right) => {
    const severityComparison = severityOrder[left.severity] - severityOrder[right.severity];
    if (severityComparison !== 0) {
      return severityComparison;
    }

    return left.title.localeCompare(right.title, "en", { sensitivity: "base" });
  });
}

function combineFindings(...buckets: readonly (readonly FindingDefinition[])[]): readonly FindingDefinition[] {
  const merged = new Map<string, FindingDefinition>();

  for (const bucket of buckets) {
    for (const finding of bucket) {
      merged.set(finding.id, finding);
    }
  }

  return sortFindings([...merged.values()]);
}

function toGeneratedReportArtifacts(reportArtifacts: readonly SharedContractPayload["reports"][number][]): readonly GeneratedArtifact[] {
  return reportArtifacts.map((artifact) => ({
    id: artifact.id,
    title: artifact.title,
    kind: "Report",
    relativePath: artifact.relativePath
  }));
}

function combineGeneratedArtifacts(...buckets: readonly (readonly GeneratedArtifact[])[]): readonly GeneratedArtifact[] {
  const merged = new Map<string, GeneratedArtifact>();

  for (const bucket of buckets) {
    for (const artifact of bucket) {
      merged.set(artifact.relativePath, artifact);
    }
  }

  return sortGeneratedArtifacts([...merged.values()]);
}

function createSensitiveDataFinding(
  classification: SensitiveDataClassification,
  index: number
): FindingDefinition {
  const categoryTitle = classification.category.replace("ChildData", "Child Data");
  const severity =
    classification.category === "Financial"
      || classification.category === "Health"
      || classification.category === "ChildData"
      ? "High"
      : "Medium";
  const evidence = classification.evidence[0] ?? "Sensitive data was detected in the repository.";

  return {
    id: `sensitive-data-${classification.category.toLowerCase()}-${index}`,
    title: `${categoryTitle} data detected`,
    summary: evidence,
    severity,
    risk: severity,
    remediation: {
      title: `Review ${categoryTitle.toLowerCase()} data handling`,
      summary: `Confirm storage, access, and retention controls for ${categoryTitle.toLowerCase()} data in the affected files.`
    },
    evidence: classification.affectedPaths
  };
}

function deriveSensitiveDataFindings(repositoryAnalysis: RepositoryAnalysisResult): readonly FindingDefinition[] {
  return repositoryAnalysis.sensitiveData.map((classification, index) => createSensitiveDataFinding(classification, index));
}

function deriveGraphEdges(
  architectureEvaluation: WorkspaceArchitectureEvaluationResult
): readonly GraphEdgeDefinition[] {
  const missingRequirementEdges = architectureEvaluation.technologyEvaluation.missingRequirements.map((requirement) => ({
    sourceId: requirement.sourceNodeId,
    targetId: requirement.requiredNodeId,
    relationship: "Requires" as const
  }));
  const conflictEdges = architectureEvaluation.technologyEvaluation.conflicts.map((conflict) => ({
    sourceId: conflict.leftNodeId,
    targetId: conflict.rightNodeId,
    relationship: "Conflicts" as const
  }));
  const recommendationEdges = architectureEvaluation.technologyEvaluation.recommendations.map((recommendation) => ({
    sourceId: recommendation.sourceNodeId,
    targetId: recommendation.nodeId,
    relationship: recommendation.relationship
  }));

  return [...missingRequirementEdges, ...conflictEdges, ...recommendationEdges].sort((left, right) => {
    const sourceComparison = left.sourceId.localeCompare(right.sourceId, "en", { sensitivity: "base" });
    if (sourceComparison !== 0) {
      return sourceComparison;
    }

    const targetComparison = left.targetId.localeCompare(right.targetId, "en", { sensitivity: "base" });
    if (targetComparison !== 0) {
      return targetComparison;
    }

    return left.relationship.localeCompare(right.relationship, "en", { sensitivity: "base" });
  });
}

function getDefaultSelection(): ProjectSelectionProfile {
  return createEmptySharedContractPayload().projectSelection;
}

export async function createLiveDashboardState(services: DashboardDataServices): Promise<DashboardState> {
  const workspacePath = await services.getWorkspaceFolder?.();

  if (!workspacePath) {
    return createDashboardState(createEmptySharedContractPayload(), {
      hasWorkspace: false,
      repositoryAnalysis: emptyRepositoryAnalysis,
      subtitle: "Open a workspace folder to load live Architecture Studio data."
    });
  }

  const [
    repositoryAnalysis,
    composedStandards,
    architectureEvaluation,
    complianceEvaluation,
    reportGeneration,
    projectSelection,
    aiInstructionContext
  ] = await Promise.all([
    services.runRepositoryAnalysis?.(workspacePath) ?? emptyRepositoryAnalysis,
    services.runStandardsComposition?.(workspacePath) ?? { standards: [], consumerHints: [] },
    services.runArchitectureEvaluation?.(workspacePath) ?? {
      technologyEvaluation: {
        selectedNodes: [],
        missingRequirements: [],
        conflicts: [],
        recommendations: []
      },
      findings: []
    },
    services.runComplianceEvaluation?.(workspacePath) ?? { summaries: [], findings: [] },
    services.runReportGeneration?.(workspacePath) ?? { reportArtifacts: [], files: [], pdfFallbackUsed: false },
    services.getProjectSelection?.() ?? undefined,
    services.getAiInstructionContext?.(workspacePath) ?? undefined
  ]);

  const hasProjectSelection = projectSelection !== undefined;
  const effectiveSelection = projectSelection ?? getDefaultSelection();
  const [projectGeneration, aiInstructionGeneration] = await Promise.all([
    hasProjectSelection && services.runProjectGeneration
      ? services.runProjectGeneration(effectiveSelection)
      : undefined,
    aiInstructionContext && services.runAiInstructionGeneration
      ? services.runAiInstructionGeneration(aiInstructionContext)
      : undefined
  ]);

  const reportArtifacts = sortById(reportGeneration.reportArtifacts);
  const findings = combineFindings(
    deriveSensitiveDataFindings(repositoryAnalysis),
    architectureEvaluation.findings,
    complianceEvaluation.findings
  );
  const payload = createEmptySharedContractPayload({
    standards: composedStandards.standards.map((entry) => entry.definition),
    graphNodes: sortById(architectureEvaluation.technologyEvaluation.selectedNodes),
    graphEdges: deriveGraphEdges(architectureEvaluation),
    complianceSummaries: sortComplianceSummaries(complianceEvaluation.summaries),
    findings,
    reports: reportArtifacts,
    generatedArtifacts: combineGeneratedArtifacts(
      toGeneratedReportArtifacts(reportArtifacts),
      projectGeneration?.generatedArtifacts ?? [],
      aiInstructionGeneration?.generatedArtifacts ?? []
    ),
    projectSelection: effectiveSelection
  });

  return createDashboardState(payload, {
    hasWorkspace: true,
    repositoryAnalysis,
    workspaceLabel: getWorkspaceLabel(workspacePath)
  });
}
