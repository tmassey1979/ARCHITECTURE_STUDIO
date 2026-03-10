namespace ArchitectureStudio.Core;

public sealed record StandardDefinition(
    string Id,
    string Title,
    StandardCategory Category,
    string Summary,
    IReadOnlyList<string> Tags);

public sealed record RegulationDefinition(
    string Id,
    RegulationCategory Category,
    string Jurisdiction,
    IReadOnlyList<string> RequiredControls,
    IReadOnlyList<string> DataTypes);

public sealed record ControlDefinition(
    string Id,
    string Title,
    string Summary);

public sealed record GraphNodeDefinition(
    string Id,
    string Label,
    GraphNodeCategory Category);

public sealed record GraphEdgeDefinition(
    string SourceId,
    string TargetId,
    GraphRelationship Relationship);

public sealed record RemediationDefinition(
    string Title,
    string Summary,
    IReadOnlyList<string>? Steps = null);

public sealed record FindingDefinition(
    string Id,
    string Title,
    string Summary,
    SeverityLevel Severity,
    RiskLevel Risk,
    RemediationDefinition Remediation,
    IReadOnlyList<string>? Evidence = null);

public sealed record ReportArtifact(
    string Id,
    string Title,
    ArtifactFormat Format,
    string RelativePath);

public sealed record GeneratedArtifact(
    string Id,
    string Title,
    GeneratedArtifactKind Kind,
    string RelativePath);

public sealed record ProjectSelectionProfile(
    string Frontend,
    string Backend,
    string ArchitecturePattern,
    IReadOnlyList<string> CiCd,
    IReadOnlyList<string> Infrastructure,
    IReadOnlyList<string> ComplianceTargets);

public sealed record SharedContractPayload(
    IReadOnlyList<StandardDefinition> Standards,
    IReadOnlyList<RegulationDefinition> Regulations,
    IReadOnlyList<ControlDefinition> Controls,
    IReadOnlyList<GraphNodeDefinition> GraphNodes,
    IReadOnlyList<GraphEdgeDefinition> GraphEdges,
    IReadOnlyList<FindingDefinition> Findings,
    IReadOnlyList<ReportArtifact> Reports,
    IReadOnlyList<GeneratedArtifact> GeneratedArtifacts,
    ProjectSelectionProfile ProjectSelection);
