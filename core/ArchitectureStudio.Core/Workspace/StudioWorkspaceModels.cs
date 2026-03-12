namespace ArchitectureStudio.Core;

public sealed record WorkspaceArchitectureEvaluationResult(
    TechnologyEvaluationResult TechnologyEvaluation,
    IReadOnlyList<FindingDefinition> Findings);
