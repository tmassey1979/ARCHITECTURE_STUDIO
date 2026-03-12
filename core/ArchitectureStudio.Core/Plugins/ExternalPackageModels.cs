using System.Text.Json.Serialization;

namespace ArchitectureStudio.Core;

public enum ExternalPackageStatusKind
{
    Loaded,
    Invalid
}

public enum ExternalPackageContributionKind
{
    Standards,
    Compliance,
    Templates,
    Graph
}

public sealed record ExternalPackageContributionReference(
    string RelativePath,
    string FullPath);

public sealed record ExternalPackageContributions(
    IReadOnlyList<ExternalPackageContributionReference> StandardsPackages,
    IReadOnlyList<ExternalPackageContributionReference> Regulations,
    IReadOnlyList<ExternalPackageContributionReference> Controls,
    IReadOnlyList<ExternalPackageContributionReference> Templates,
    IReadOnlyList<ExternalPackageContributionReference> GraphDatasets);

public sealed record ExternalPackage(
    string Id,
    string Version,
    string SchemaVersion,
    string DisplayName,
    string RootPath,
    ExternalPackageContributions Contributions);

public sealed record ExternalPackageStatus(
    string PackageId,
    ExternalPackageStatusKind Status,
    string Message,
    IReadOnlyList<ExternalPackageContributionKind> ContributionKinds);

public sealed record ExternalPackageDiscoveryResult(
    IReadOnlyList<ExternalPackage> Packages,
    IReadOnlyList<ExternalPackageStatus> Statuses);

internal sealed record ExternalPackageManifest(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("version")] string Version,
    [property: JsonPropertyName("schemaVersion")] string SchemaVersion,
    [property: JsonPropertyName("displayName")] string DisplayName,
    [property: JsonPropertyName("contributions")] ExternalPackageManifestContributions Contributions);

internal sealed record ExternalPackageManifestContributions(
    [property: JsonPropertyName("standardsPackages")] IReadOnlyList<string>? StandardsPackages,
    [property: JsonPropertyName("regulations")] IReadOnlyList<string>? Regulations,
    [property: JsonPropertyName("controls")] IReadOnlyList<string>? Controls,
    [property: JsonPropertyName("templates")] IReadOnlyList<string>? Templates,
    [property: JsonPropertyName("graphDatasets")] IReadOnlyList<string>? GraphDatasets);
