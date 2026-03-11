namespace ArchitectureStudio.Core.Tests;

public class TechnologyGraphEngineTests
{
    [Fact]
    public void Default_graph_dataset_covers_required_node_and_edge_types_with_broad_curated_categories()
    {
        var graph = TechnologyGraphCatalog.CreateDefault().Graph;

        Assert.Equal(
            Enum.GetValues<GraphNodeCategory>().OrderBy(static category => category.ToString()).ToArray(),
            graph.Nodes.Select(static node => node.Category).Distinct().OrderBy(static category => category.ToString()).ToArray());

        Assert.Equal(
            Enum.GetValues<GraphRelationship>().OrderBy(static relationship => relationship.ToString()).ToArray(),
            graph.Edges.Select(static edge => edge.Relationship).Distinct().OrderBy(static relationship => relationship.ToString()).ToArray());

        var ids = graph.Nodes.Select(static node => node.Id).ToHashSet(StringComparer.Ordinal);

        foreach (var expectedId in new[]
        {
            "react",
            "vue",
            "wpf",
            "blazor",
            "aspnet-core",
            "spring-boot",
            "django",
            "fastapi",
            "aws",
            "azure",
            "gcp",
            "postgresql",
            "mongodb",
            "github-actions",
            "jenkins",
            "kafka",
            "rabbitmq",
            "prometheus",
            "grafana",
            "opentelemetry",
            "vault",
            "keycloak",
            "clean-architecture",
            "hexagonal-architecture",
            "onion-architecture",
            "layered-architecture",
            "vertical-slice-architecture",
            "microservices",
            "event-driven-architecture",
            "gdpr",
            "audit-logging"
        })
        {
            Assert.Contains(expectedId, ids);
        }
    }

    [Fact]
    public void Architecture_patterns_named_in_studio_are_shipped_in_the_graph_dataset()
    {
        var graph = TechnologyGraphCatalog.CreateDefault().Graph;

        foreach (var patternId in new[]
        {
            "clean-architecture",
            "hexagonal-architecture",
            "onion-architecture",
            "layered-architecture",
            "vertical-slice-architecture",
            "microservices",
            "event-driven-architecture"
        })
        {
            Assert.Contains(graph.Nodes, node => node.Id == patternId && node.Category == GraphNodeCategory.ArchitecturePattern);
        }
    }

    [Fact]
    public void Graph_engine_evaluates_compatibility_and_recommendations_from_a_selected_stack()
    {
        var engine = new TechnologyGraphEngine(TechnologyGraphCatalog.CreateDefault());
        var result = engine.Evaluate(
            new TechnologyStackSelection(
                SelectedNodeIds: ["react", "aspnet-core", "docker", "wpf"]));

        Assert.Contains(result.MissingRequirements, item => item.RequiredNodeId == "javascript");
        Assert.Contains(result.MissingRequirements, item => item.RequiredNodeId == "dotnet");
        Assert.Contains(result.Recommendations, item => item.NodeId == "rest-api");
        Assert.Contains(result.Recommendations, item => item.NodeId == "cqrs");
        Assert.Contains(result.Recommendations, item => item.NodeId == "helm");
        Assert.Contains(result.Conflicts, item => item.LeftNodeId == "react" && item.RightNodeId == "wpf");
    }

    [Fact]
    public void Architecture_validation_emits_findings_for_the_required_violation_set()
    {
        var engine = new TechnologyGraphEngine(TechnologyGraphCatalog.CreateDefault());
        var findings = engine.ValidateArchitecture(
            new ArchitectureValidationRequest(
                DomainToInfrastructureReferences: ["Domain/Orders/OrderService.cs -> Infrastructure/SqlOrderRepository.cs"],
                UiBusinessLogicFiles: ["ui/dashboard/orderTotals.ts"],
                ControllerDatabaseAccesses: ["OrdersController.cs -> dbContext.Orders"],
                AuthenticationConfigured: false));

        Assert.Contains(findings, finding => finding.Id == "domain-references-infrastructure");
        Assert.Contains(findings, finding => finding.Id == "business-logic-in-ui");
        Assert.Contains(findings, finding => finding.Id == "direct-database-access-from-controller");
        Assert.Contains(findings, finding => finding.Id == "missing-authentication");
        Assert.All(findings, static finding => Assert.NotNull(finding.Remediation));
    }
}
