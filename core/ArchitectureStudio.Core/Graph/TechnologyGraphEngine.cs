namespace ArchitectureStudio.Core;

public sealed class TechnologyGraphEngine
{
    private readonly IReadOnlyDictionary<string, GraphNodeDefinition> _nodesById;
    private readonly IReadOnlyDictionary<string, IReadOnlyList<GraphEdgeDefinition>> _edgesBySourceId;

    public TechnologyGraphEngine(TechnologyGraphCatalog catalog)
    {
        _nodesById = catalog.Graph.Nodes.ToDictionary(static node => node.Id, StringComparer.OrdinalIgnoreCase);
        _edgesBySourceId = catalog.Graph.Edges
            .GroupBy(static edge => edge.SourceId, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(
                static group => group.Key,
                static group => (IReadOnlyList<GraphEdgeDefinition>)group.ToArray(),
                StringComparer.OrdinalIgnoreCase);
    }

    public TechnologyEvaluationResult Evaluate(TechnologyStackSelection selection)
    {
        var selectedIds = selection.SelectedNodeIds
            .Where(static id => !string.IsNullOrWhiteSpace(id))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var selectedNodes = selectedIds
            .Where(_nodesById.ContainsKey)
            .Select(id => _nodesById[id])
            .OrderBy(static node => node.Category)
            .ThenBy(static node => node.Label, StringComparer.Ordinal)
            .ThenBy(static node => node.Id, StringComparer.Ordinal)
            .ToArray();

        var missingRequirements = new HashSet<TechnologyMissingRequirement>();
        var conflicts = new HashSet<TechnologyConflict>();
        var recommendations = new HashSet<TechnologyRecommendation>();

        foreach (var selectedId in selectedIds)
        {
            if (!_edgesBySourceId.TryGetValue(selectedId, out var edges))
            {
                continue;
            }

            foreach (var edge in edges)
            {
                switch (edge.Relationship)
                {
                    case GraphRelationship.Requires when !selectedIds.Contains(edge.TargetId):
                        missingRequirements.Add(new TechnologyMissingRequirement(edge.SourceId, edge.TargetId));
                        break;
                    case GraphRelationship.Conflicts when selectedIds.Contains(edge.TargetId):
                        conflicts.Add(CreateConflict(edge.SourceId, edge.TargetId));
                        break;
                    case GraphRelationship.PairsWith when !selectedIds.Contains(edge.TargetId):
                    case GraphRelationship.RecommendedWith when !selectedIds.Contains(edge.TargetId):
                        recommendations.Add(new TechnologyRecommendation(edge.SourceId, edge.TargetId, edge.Relationship));
                        break;
                }
            }
        }

        return new TechnologyEvaluationResult(
            SelectedNodes: selectedNodes,
            MissingRequirements: missingRequirements
                .OrderBy(static item => item.SourceNodeId, StringComparer.Ordinal)
                .ThenBy(static item => item.RequiredNodeId, StringComparer.Ordinal)
                .ToArray(),
            Conflicts: conflicts
                .OrderBy(static item => item.LeftNodeId, StringComparer.Ordinal)
                .ThenBy(static item => item.RightNodeId, StringComparer.Ordinal)
                .ToArray(),
            Recommendations: recommendations
                .OrderBy(static item => item.SourceNodeId, StringComparer.Ordinal)
                .ThenBy(static item => item.NodeId, StringComparer.Ordinal)
                .ThenBy(static item => item.Relationship)
                .ToArray());
    }

    public IReadOnlyList<FindingDefinition> ValidateArchitecture(ArchitectureValidationRequest request)
    {
        var findings = new List<FindingDefinition>();

        if (request.DomainToInfrastructureReferences.Count > 0)
        {
            findings.Add(
                CreateFinding(
                    id: "domain-references-infrastructure",
                    title: "Domain references infrastructure",
                    summary: "Domain code should not depend directly on infrastructure implementation details.",
                    severity: SeverityLevel.High,
                    risk: RiskLevel.High,
                    remediationTitle: "Move infrastructure references behind an abstraction",
                    remediationSummary: "Introduce ports or interfaces so domain code depends on abstractions rather than infrastructure implementations.",
                    evidence: request.DomainToInfrastructureReferences));
        }

        if (request.UiBusinessLogicFiles.Count > 0)
        {
            findings.Add(
                CreateFinding(
                    id: "business-logic-in-ui",
                    title: "Business logic in UI layer",
                    summary: "UI composition should not own business rules that belong in application or domain layers.",
                    severity: SeverityLevel.Medium,
                    risk: RiskLevel.Medium,
                    remediationTitle: "Move business logic into application services",
                    remediationSummary: "Keep UI components thin and move domain decisions into testable service or domain layers.",
                    evidence: request.UiBusinessLogicFiles));
        }

        if (request.ControllerDatabaseAccesses.Count > 0)
        {
            findings.Add(
                CreateFinding(
                    id: "direct-database-access-from-controller",
                    title: "Direct database access from controller",
                    summary: "Controllers should orchestrate requests, not perform direct data access.",
                    severity: SeverityLevel.High,
                    risk: RiskLevel.High,
                    remediationTitle: "Route data access through application or repository services",
                    remediationSummary: "Move direct database calls behind application services or repositories to preserve architectural boundaries.",
                    evidence: request.ControllerDatabaseAccesses));
        }

        if (!request.AuthenticationConfigured)
        {
            findings.Add(
                CreateFinding(
                    id: "missing-authentication",
                    title: "Missing authentication",
                    summary: "The architecture is missing an authentication boundary for protected operations.",
                    severity: SeverityLevel.Critical,
                    risk: RiskLevel.Critical,
                    remediationTitle: "Add an authentication provider",
                    remediationSummary: "Introduce an authentication flow such as OAuth or an identity provider before exposing protected functionality.",
                    evidence: ["Authentication middleware or identity provider not configured."]));
        }

        return findings;
    }

    private static TechnologyConflict CreateConflict(string leftNodeId, string rightNodeId)
    {
        return string.Compare(leftNodeId, rightNodeId, StringComparison.OrdinalIgnoreCase) <= 0
            ? new TechnologyConflict(leftNodeId, rightNodeId)
            : new TechnologyConflict(rightNodeId, leftNodeId);
    }

    private static FindingDefinition CreateFinding(
        string id,
        string title,
        string summary,
        SeverityLevel severity,
        RiskLevel risk,
        string remediationTitle,
        string remediationSummary,
        IReadOnlyList<string> evidence)
    {
        return new FindingDefinition(
            Id: id,
            Title: title,
            Summary: summary,
            Severity: severity,
            Risk: risk,
            Remediation: new RemediationDefinition(
                Title: remediationTitle,
                Summary: remediationSummary),
            Evidence: evidence);
    }
}
