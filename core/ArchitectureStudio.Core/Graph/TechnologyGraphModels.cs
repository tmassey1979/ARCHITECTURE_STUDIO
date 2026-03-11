namespace ArchitectureStudio.Core;

public sealed record TechnologyGraph(
    IReadOnlyList<GraphNodeDefinition> Nodes,
    IReadOnlyList<GraphEdgeDefinition> Edges);

public sealed record TechnologyStackSelection(
    IReadOnlyList<string> SelectedNodeIds);

public sealed record TechnologyMissingRequirement(
    string SourceNodeId,
    string RequiredNodeId);

public sealed record TechnologyConflict(
    string LeftNodeId,
    string RightNodeId);

public sealed record TechnologyRecommendation(
    string SourceNodeId,
    string NodeId,
    GraphRelationship Relationship);

public sealed record TechnologyEvaluationResult(
    IReadOnlyList<GraphNodeDefinition> SelectedNodes,
    IReadOnlyList<TechnologyMissingRequirement> MissingRequirements,
    IReadOnlyList<TechnologyConflict> Conflicts,
    IReadOnlyList<TechnologyRecommendation> Recommendations);

public sealed record ArchitectureValidationRequest(
    IReadOnlyList<string> DomainToInfrastructureReferences,
    IReadOnlyList<string> UiBusinessLogicFiles,
    IReadOnlyList<string> ControllerDatabaseAccesses,
    bool AuthenticationConfigured);
