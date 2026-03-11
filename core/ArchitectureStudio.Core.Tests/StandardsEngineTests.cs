using System.Text.Json;

namespace ArchitectureStudio.Core.Tests;

public class StandardsEngineTests
{
    [Fact]
    public void Seed_library_covers_all_required_standard_categories_and_named_examples()
    {
        var catalog = StandardsCatalog.CreateDefault();

        Assert.Equal(
            Enum.GetValues<StandardCategory>().OrderBy(static category => category.ToString()).ToArray(),
            catalog.Standards.Select(static standard => standard.Category).Distinct().OrderBy(static category => category.ToString()).ToArray());

        var ids = catalog.Standards.Select(static standard => standard.Id).ToHashSet(StringComparer.Ordinal);
        var expectedIds = new[]
        {
            "clean-code",
            "solid",
            "dry",
            "kiss",
            "yagni",
            "cqrs",
            "event-sourcing",
            "react",
            "angular",
            "vue",
            "wpf",
            "react-native",
            "rest",
            "graphql",
            "grpc",
            "docker",
            "kubernetes",
            "terraform",
            "helm",
            "github-actions",
            "gitlab-ci",
            "jenkins",
            "bamboo",
            "circleci",
            "azure-pipelines"
        };

        foreach (var expectedId in expectedIds)
        {
            Assert.Contains(expectedId, ids);
        }
    }

    [Fact]
    public void Composition_is_deterministic_and_uses_project_selection_and_repository_characteristics()
    {
        var engine = new StandardsCompositionEngine(StandardsCatalog.CreateDefault());
        var request = new StandardsCompositionRequest(
            ProjectSelection: new StandardsProjectSelection(
                Frontend: "React",
                Backend: "gRPC",
                ArchitecturePattern: "CQRS",
                CiCd: ["GitHub Actions"],
                Infrastructure: ["Docker", "Kubernetes"],
                AdditionalSelections: ["SOLID"]),
            RepositoryCharacteristics: new RepositoryCharacteristics(
                DetectedTechnologies: ["React", "GraphQL", "OpenTelemetry"],
                DetectedTags: ["security", "testing"],
                DetectedCategories: [StandardCategory.Frontend, StandardCategory.Security]));

        var first = engine.Compose(request);
        var second = engine.Compose(request);

        Assert.Equal(
            first.Standards.Select(static standard => standard.Definition.Id).ToArray(),
            second.Standards.Select(static standard => standard.Definition.Id).ToArray());

        Assert.Contains(first.Standards, static standard => standard.Definition.Id == "react");
        Assert.Contains(first.Standards, static standard => standard.Definition.Id == "grpc");
        Assert.Contains(first.Standards, static standard => standard.Definition.Id == "cqrs");
        Assert.Contains(first.Standards, static standard => standard.Definition.Id == "github-actions");
        Assert.Contains(first.Standards, static standard => standard.Definition.Id == "docker");
        Assert.Contains(first.Standards, static standard => standard.Definition.Id == "kubernetes");
        Assert.Contains(first.Standards, static standard => standard.Definition.Id == "solid");
        Assert.Contains(first.Standards, static standard => standard.Definition.Id == "graphql");
        Assert.Contains(first.Standards, static standard => standard.Definition.Id == "threat-modeling");
        Assert.Contains(first.Standards, static standard => standard.Definition.Id == "test-pyramid");

        var graphql = Assert.Single(first.Standards, static standard => standard.Definition.Id == "graphql");
        Assert.Contains("Detected technology: GraphQL", graphql.SelectionReasons);
        Assert.Equal("architecture-studio.seed", graphql.Source.PackageId);
    }

    [Fact]
    public void External_packages_extend_the_catalog_without_rewriting_the_engine()
    {
        var externalPackage = new StandardsPackage(
            Id: "custom.partner",
            Version: "1.0.0",
            SourcePath: "standards/packages/custom.partner.json",
            Standards:
            [
                new StandardsLibraryEntry(
                    Definition: new StandardDefinition(
                        Id: "hot-chocolate",
                        Title: "Hot Chocolate",
                        Category: StandardCategory.Backend,
                        Summary: "GraphQL server for .NET APIs.",
                        Tags: ["graphql", "c#", ".net"]),
                    AppliesToCategories: [StandardCategory.Backend],
                    AppliesToSelections: ["graphql", ".net", "c#"],
                    AppliesToTags: [],
                    SourceTitle: "Partner Standards Pack")
            ]);

        var catalog = StandardsCatalog.CreateDefault().WithPackage(externalPackage);
        var engine = new StandardsCompositionEngine(catalog);
        var result = engine.Compose(
            new StandardsCompositionRequest(
                ProjectSelection: new StandardsProjectSelection(
                    Frontend: "",
                    Backend: ".NET",
                    ArchitecturePattern: "",
                    CiCd: [],
                    Infrastructure: [],
                    AdditionalSelections: ["GraphQL"])));

        var custom = Assert.Single(result.Standards, static standard => standard.Definition.Id == "hot-chocolate");

        Assert.Equal("custom.partner", custom.Source.PackageId);
        Assert.Contains("Explicit selection: GraphQL", custom.SelectionReasons);
    }

    [Fact]
    public void Composition_result_round_trips_through_json_for_downstream_transport()
    {
        var engine = new StandardsCompositionEngine(StandardsCatalog.CreateDefault());
        var result = engine.Compose(
            new StandardsCompositionRequest(
                ProjectSelection: new StandardsProjectSelection(
                    Frontend: "Vue",
                    Backend: "REST",
                    ArchitecturePattern: "Domain Driven Design",
                    CiCd: ["Azure Pipelines"],
                    Infrastructure: ["Terraform"],
                    AdditionalSelections: ["Clean Code"])));

        var json = JsonSerializer.Serialize(result, StandardsJson.SerializerOptions);
        var roundTrip = JsonSerializer.Deserialize<ComposedStandardsResult>(json, StandardsJson.SerializerOptions);

        Assert.NotNull(roundTrip);
        Assert.NotEmpty(roundTrip.Standards);
        Assert.Contains(roundTrip.ConsumerHints, static hint => hint.Workflow == StandardsConsumerWorkflow.Dashboard);
        Assert.Contains(roundTrip.ConsumerHints, static hint => hint.Workflow == StandardsConsumerWorkflow.AiInstructions);
    }
}
