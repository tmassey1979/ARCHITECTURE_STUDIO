namespace ArchitectureStudio.Core;

public enum AiInstructionTargetKind
{
    GeneratedProject,
    AnalyzedRepository
}

public sealed record AiInstructionGenerationRequest(
    string ProjectName,
    AiInstructionTargetKind TargetKind,
    ProjectSelectionProfile ProjectSelection,
    IReadOnlyList<StandardDefinition> Standards,
    IReadOnlyList<ComplianceSummary> ComplianceSummaries,
    IReadOnlyList<FindingDefinition> Findings);

public sealed record AiInstructionGeneratedFile(
    string RelativePath,
    string Title,
    GeneratedArtifactKind Kind,
    string Content);

public sealed record AiInstructionGenerationResult(
    IReadOnlyList<GeneratedArtifact> GeneratedArtifacts,
    IReadOnlyList<AiInstructionGeneratedFile> Files);
