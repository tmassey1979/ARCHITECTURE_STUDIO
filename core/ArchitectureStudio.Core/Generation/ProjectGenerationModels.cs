using System.Text.Json.Serialization;

namespace ArchitectureStudio.Core;

public sealed record ProjectTemplateFile(
    [property: JsonPropertyName("relative_path")] string RelativePath,
    [property: JsonPropertyName("artifact_kind")] GeneratedArtifactKind ArtifactKind,
    [property: JsonPropertyName("title")] string Title,
    [property: JsonPropertyName("content")] string Content);

public sealed record ProjectTemplateDefinition(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("selection_key")] string SelectionKey,
    [property: JsonPropertyName("selection_values")] IReadOnlyList<string> SelectionValues,
    [property: JsonPropertyName("description")] string Description,
    [property: JsonPropertyName("files")] IReadOnlyList<ProjectTemplateFile> Files);

public sealed record ProjectGeneratedFile(
    string RelativePath,
    string Title,
    GeneratedArtifactKind Kind,
    string Content);

public sealed record ProjectGenerationResult(
    IReadOnlyList<string> TemplateIds,
    IReadOnlyList<GeneratedArtifact> GeneratedArtifacts,
    IReadOnlyList<ProjectGeneratedFile> Files);
