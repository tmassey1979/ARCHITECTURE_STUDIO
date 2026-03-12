using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace ArchitectureStudio.Core;

public sealed class TechnologyGraphCatalog
{
    private const string RelativeDatasetDirectory = "Graph/Datasets";

    private readonly TechnologyGraph _graph;

    private TechnologyGraphCatalog(TechnologyGraph graph)
    {
        _graph = graph;
    }

    public TechnologyGraph Graph => _graph;

    public static TechnologyGraphCatalog CreateDefault()
    {
        return StudioRuntimeCatalogFactory.CreateDefault().TechnologyGraphCatalog;
    }

    internal static TechnologyGraphCatalog CreateBuiltIn()
    {
        var datasetDirectory = Path.Combine(AppContext.BaseDirectory, RelativeDatasetDirectory);
        if (!Directory.Exists(datasetDirectory))
        {
            throw new InvalidOperationException($"Graph dataset directory '{datasetDirectory}' was not found.");
        }

        var yamlFiles = Directory.GetFiles(datasetDirectory, "*.yml", SearchOption.TopDirectoryOnly)
            .OrderBy(static path => path, StringComparer.Ordinal)
            .ToArray();

        return new TechnologyGraphCatalog(LoadGraph(yamlFiles));
    }

    internal TechnologyGraphCatalog WithExternalPackage(ExternalPackage package)
    {
        if (package.Contributions.GraphDatasets.Count == 0)
        {
            return this;
        }

        var externalGraph = LoadGraph(package.Contributions.GraphDatasets.Select(static reference => reference.FullPath));
        var nodes = _graph.Nodes
            .Concat(externalGraph.Nodes)
            .GroupBy(static node => node.Id, StringComparer.OrdinalIgnoreCase)
            .Select(static group => group.Last())
            .OrderBy(static node => node.Category)
            .ThenBy(static node => node.Label, StringComparer.Ordinal)
            .ThenBy(static node => node.Id, StringComparer.Ordinal)
            .ToArray();
        var edges = _graph.Edges
            .Concat(externalGraph.Edges)
            .Distinct()
            .OrderBy(static edge => edge.SourceId, StringComparer.Ordinal)
            .ThenBy(static edge => edge.TargetId, StringComparer.Ordinal)
            .ThenBy(static edge => edge.Relationship)
            .ToArray();

        return new TechnologyGraphCatalog(new TechnologyGraph(nodes, edges));
    }

    private static TechnologyGraph LoadGraph(IEnumerable<string> yamlFiles)
    {
        var datasetNodes = yamlFiles
            .SelectMany(ReadDatasetNodes)
            .ToArray();

        var nodes = datasetNodes
            .Select(static node => new GraphNodeDefinition(node.Id, node.Label, node.Category))
            .GroupBy(static node => node.Id, StringComparer.OrdinalIgnoreCase)
            .Select(static group => group.Last())
            .OrderBy(static node => node.Category)
            .ThenBy(static node => node.Label, StringComparer.Ordinal)
            .ThenBy(static node => node.Id, StringComparer.Ordinal)
            .ToArray();
        var edges = datasetNodes
            .SelectMany(CreateEdges)
            .Distinct()
            .OrderBy(static edge => edge.SourceId, StringComparer.Ordinal)
            .ThenBy(static edge => edge.TargetId, StringComparer.Ordinal)
            .ThenBy(static edge => edge.Relationship)
            .ToArray();

        return new TechnologyGraph(nodes, edges);
    }

    private static IReadOnlyList<TechnologyGraphDatasetNode> ReadDatasetNodes(string path)
    {
        var deserializer = new DeserializerBuilder()
            .WithNamingConvention(UnderscoredNamingConvention.Instance)
            .IgnoreUnmatchedProperties()
            .Build();

        using var reader = File.OpenText(path);
        var document = deserializer.Deserialize<TechnologyGraphDatasetDocument>(reader);
        return document.Nodes ?? [];
    }

    private static IEnumerable<GraphEdgeDefinition> CreateEdges(TechnologyGraphDatasetNode node)
    {
        return CreateEdges(node.Id, node.Requires, GraphRelationship.Requires)
            .Concat(CreateEdges(node.Id, node.Conflicts, GraphRelationship.Conflicts))
            .Concat(CreateEdges(node.Id, node.PairsWith, GraphRelationship.PairsWith))
            .Concat(CreateEdges(node.Id, node.RecommendedWith, GraphRelationship.RecommendedWith));
    }

    private static IEnumerable<GraphEdgeDefinition> CreateEdges(
        string sourceId,
        IReadOnlyList<string>? targets,
        GraphRelationship relationship)
    {
        return (targets ?? [])
            .Select(targetId => new GraphEdgeDefinition(sourceId, targetId, relationship));
    }

    private sealed class TechnologyGraphDatasetDocument
    {
        public List<TechnologyGraphDatasetNode>? Nodes { get; init; }
    }

    private sealed class TechnologyGraphDatasetNode
    {
        public string Id { get; init; } = string.Empty;

        public string Label { get; init; } = string.Empty;

        public GraphNodeCategory Category { get; init; }

        public List<string>? Requires { get; init; }

        public List<string>? Conflicts { get; init; }

        public List<string>? PairsWith { get; init; }

        public List<string>? RecommendedWith { get; init; }
    }
}
