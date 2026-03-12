using System.Text.Json;
using System.Text.Json.Serialization;
using YamlDotNet.Core;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace ArchitectureStudio.Core;

public static class ExternalPackageLoader
{
    private const string ManifestFileName = "architecture-studio.package.json";

    private static readonly JsonSerializerOptions SerializerOptions = CreateSerializerOptions();

    public static ExternalPackageDiscoveryResult Discover(string packagesRoot)
    {
        if (!Directory.Exists(packagesRoot))
        {
            return new ExternalPackageDiscoveryResult([], []);
        }

        var packages = new List<ExternalPackage>();
        var statuses = new List<ExternalPackageStatus>();
        var loadedPackageIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var packageDirectory in Directory.GetDirectories(packagesRoot, "*", SearchOption.TopDirectoryOnly)
                     .OrderBy(static path => path, StringComparer.Ordinal))
        {
            var manifestPath = Path.Combine(packageDirectory, ManifestFileName);
            if (!File.Exists(manifestPath))
            {
                continue;
            }

            try
            {
                var package = LoadPackage(packageDirectory, manifestPath);
                if (!loadedPackageIds.Add(package.Id))
                {
                    throw new InvalidOperationException($"Package '{package.Id}' is declared more than once.");
                }

                packages.Add(package);
                statuses.Add(
                    new ExternalPackageStatus(
                        package.Id,
                        ExternalPackageStatusKind.Loaded,
                        $"Loaded {CountContributionFiles(package.Contributions)} contribution file(s).",
                        GetContributionKinds(package.Contributions)));
            }
            catch (Exception exception) when (exception is InvalidOperationException or IOException or JsonException or UnauthorizedAccessException or YamlException)
            {
                statuses.Add(
                    new ExternalPackageStatus(
                        Path.GetFileName(packageDirectory),
                        ExternalPackageStatusKind.Invalid,
                        exception.Message,
                        []));
            }
        }

        return new ExternalPackageDiscoveryResult(
            packages
                .OrderBy(static package => package.Id, StringComparer.Ordinal)
                .ToArray(),
            statuses
                .OrderBy(static status => status.PackageId, StringComparer.Ordinal)
                .ToArray());
    }

    private static ExternalPackage LoadPackage(string packageDirectory, string manifestPath)
    {
        using var manifestStream = File.OpenRead(manifestPath);
        var manifest = JsonSerializer.Deserialize<ExternalPackageManifest>(manifestStream, SerializerOptions)
            ?? throw new InvalidOperationException($"Package manifest '{manifestPath}' could not be deserialized.");

        ValidateManifest(manifest, manifestPath);

        var contributions = new ExternalPackageContributions(
            LoadStandardsPackages(packageDirectory, manifest.Contributions.StandardsPackages),
            LoadRegulations(packageDirectory, manifest.Contributions.Regulations),
            LoadControls(packageDirectory, manifest.Contributions.Controls),
            LoadTemplates(packageDirectory, manifest.Contributions.Templates),
            LoadGraphDatasets(packageDirectory, manifest.Contributions.GraphDatasets));

        return new ExternalPackage(
            manifest.Id,
            manifest.Version,
            manifest.SchemaVersion,
            manifest.DisplayName,
            Path.GetFullPath(packageDirectory),
            contributions);
    }

    private static void ValidateManifest(ExternalPackageManifest manifest, string manifestPath)
    {
        if (string.IsNullOrWhiteSpace(manifest.Id)
            || string.IsNullOrWhiteSpace(manifest.Version)
            || string.IsNullOrWhiteSpace(manifest.SchemaVersion)
            || string.IsNullOrWhiteSpace(manifest.DisplayName)
            || manifest.Contributions is null)
        {
            throw new InvalidOperationException($"Package manifest '{manifestPath}' is missing required metadata.");
        }
    }

    private static IReadOnlyList<ExternalPackageContributionReference> LoadStandardsPackages(
        string packageDirectory,
        IReadOnlyList<string>? relativePaths)
    {
        return LoadContributionFiles(
            packageDirectory,
            relativePaths,
            "standards package",
            path =>
            {
                using var stream = File.OpenRead(path);
                var package = StandardsJson.Deserialize<StandardsPackage>(stream);
                if (string.IsNullOrWhiteSpace(package.Id) || string.IsNullOrWhiteSpace(package.Version))
                {
                    throw new InvalidOperationException($"Standards package '{path}' is missing required metadata.");
                }
            });
    }

    private static IReadOnlyList<ExternalPackageContributionReference> LoadRegulations(
        string packageDirectory,
        IReadOnlyList<string>? relativePaths)
    {
        return LoadContributionFiles(
            packageDirectory,
            relativePaths,
            "regulation dataset",
            path =>
            {
                var regulations = ReadJsonValues<ComplianceRegulationDefinition>(path);
                if (regulations.Count == 0)
                {
                    throw new InvalidOperationException($"Regulation dataset '{path}' does not contain any entries.");
                }

                foreach (var regulation in regulations)
                {
                    if (string.IsNullOrWhiteSpace(regulation.Id)
                        || string.IsNullOrWhiteSpace(regulation.Title)
                        || regulation.RequiredControls.Count == 0)
                    {
                        throw new InvalidOperationException($"Regulation dataset '{path}' contains an invalid entry.");
                    }
                }
            });
    }

    private static IReadOnlyList<ExternalPackageContributionReference> LoadControls(
        string packageDirectory,
        IReadOnlyList<string>? relativePaths)
    {
        return LoadContributionFiles(
            packageDirectory,
            relativePaths,
            "control dataset",
            path =>
            {
                var controls = ReadJsonValues<ComplianceControlDefinition>(path);
                if (controls.Count == 0)
                {
                    throw new InvalidOperationException($"Control dataset '{path}' does not contain any entries.");
                }

                foreach (var control in controls)
                {
                    if (string.IsNullOrWhiteSpace(control.Id)
                        || string.IsNullOrWhiteSpace(control.Title)
                        || string.IsNullOrWhiteSpace(control.RemediationTitle)
                        || string.IsNullOrWhiteSpace(control.RemediationSummary))
                    {
                        throw new InvalidOperationException($"Control dataset '{path}' contains an invalid entry.");
                    }
                }
            });
    }

    private static IReadOnlyList<ExternalPackageContributionReference> LoadTemplates(
        string packageDirectory,
        IReadOnlyList<string>? relativePaths)
    {
        return LoadContributionFiles(
            packageDirectory,
            relativePaths,
            "template",
            path =>
            {
                using var stream = File.OpenRead(path);
                var template = JsonSerializer.Deserialize<ProjectTemplateDefinition>(stream, SerializerOptions)
                    ?? throw new InvalidOperationException($"Template '{path}' could not be deserialized.");

                if (string.IsNullOrWhiteSpace(template.Id)
                    || string.IsNullOrWhiteSpace(template.SelectionKey)
                    || template.Files.Count == 0)
                {
                    throw new InvalidOperationException($"Template '{path}' is missing required schema fields.");
                }
            });
    }

    private static IReadOnlyList<ExternalPackageContributionReference> LoadGraphDatasets(
        string packageDirectory,
        IReadOnlyList<string>? relativePaths)
    {
        var deserializer = new DeserializerBuilder()
            .WithNamingConvention(UnderscoredNamingConvention.Instance)
            .IgnoreUnmatchedProperties()
            .Build();

        return LoadContributionFiles(
            packageDirectory,
            relativePaths,
            "graph dataset",
            path =>
            {
                using var reader = File.OpenText(path);
                var document = deserializer.Deserialize<ExternalGraphDatasetDocument>(reader);
                if (document.Nodes is null || document.Nodes.Count == 0)
                {
                    throw new InvalidOperationException($"Graph dataset '{path}' does not declare any nodes.");
                }

                foreach (var node in document.Nodes)
                {
                    if (string.IsNullOrWhiteSpace(node.Id) || string.IsNullOrWhiteSpace(node.Label))
                    {
                        throw new InvalidOperationException($"Graph dataset '{path}' contains an invalid node.");
                    }
                }
            });
    }

    private static IReadOnlyList<ExternalPackageContributionReference> LoadContributionFiles(
        string packageDirectory,
        IReadOnlyList<string>? relativePaths,
        string contributionLabel,
        Action<string> validateContribution)
    {
        if (relativePaths is null || relativePaths.Count == 0)
        {
            return [];
        }

        return relativePaths
            .Select(relativePath =>
            {
                var fullPath = ResolveContributionPath(packageDirectory, relativePath, contributionLabel);
                validateContribution(fullPath);
                return new ExternalPackageContributionReference(relativePath, fullPath);
            })
            .ToArray();
    }

    private static string ResolveContributionPath(string packageDirectory, string relativePath, string contributionLabel)
    {
        if (string.IsNullOrWhiteSpace(relativePath))
        {
            throw new InvalidOperationException($"A {contributionLabel} reference is blank.");
        }

        if (Path.IsPathRooted(relativePath))
        {
            throw new InvalidOperationException($"The {contributionLabel} reference '{relativePath}' must be package-relative.");
        }

        var packageRoot = Path.GetFullPath(packageDirectory);
        var fullPath = Path.GetFullPath(Path.Combine(packageRoot, relativePath.Replace('/', Path.DirectorySeparatorChar)));
        var packageRootPrefix = packageRoot.EndsWith(Path.DirectorySeparatorChar)
            ? packageRoot
            : packageRoot + Path.DirectorySeparatorChar;

        if (!fullPath.StartsWith(packageRootPrefix, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException($"The {contributionLabel} reference '{relativePath}' escapes the package root.");
        }

        if (!File.Exists(fullPath))
        {
            throw new InvalidOperationException($"{relativePath} was not found.");
        }

        return fullPath;
    }

    private static IReadOnlyList<T> ReadJsonValues<T>(string path)
    {
        using var stream = File.OpenRead(path);
        using var document = JsonDocument.Parse(stream);

        return document.RootElement.ValueKind switch
        {
            JsonValueKind.Array => document.RootElement.Deserialize<IReadOnlyList<T>>(SerializerOptions) ?? [],
            JsonValueKind.Object => document.RootElement.Deserialize<T>(SerializerOptions) is { } value ? [value] : [],
            _ => []
        };
    }

    private static IReadOnlyList<ExternalPackageContributionKind> GetContributionKinds(ExternalPackageContributions contributions)
    {
        var kinds = new List<ExternalPackageContributionKind>();

        if (contributions.StandardsPackages.Count > 0)
        {
            kinds.Add(ExternalPackageContributionKind.Standards);
        }

        if (contributions.Regulations.Count > 0 || contributions.Controls.Count > 0)
        {
            kinds.Add(ExternalPackageContributionKind.Compliance);
        }

        if (contributions.Templates.Count > 0)
        {
            kinds.Add(ExternalPackageContributionKind.Templates);
        }

        if (contributions.GraphDatasets.Count > 0)
        {
            kinds.Add(ExternalPackageContributionKind.Graph);
        }

        return kinds;
    }

    private static int CountContributionFiles(ExternalPackageContributions contributions)
    {
        return contributions.StandardsPackages.Count
               + contributions.Regulations.Count
               + contributions.Controls.Count
               + contributions.Templates.Count
               + contributions.GraphDatasets.Count;
    }

    private static JsonSerializerOptions CreateSerializerOptions()
    {
        var options = new JsonSerializerOptions(JsonSerializerDefaults.Web)
        {
            PropertyNameCaseInsensitive = true,
            ReadCommentHandling = JsonCommentHandling.Skip
        };
        options.Converters.Add(new JsonStringEnumConverter());
        return options;
    }

    private sealed class ExternalGraphDatasetDocument
    {
        public List<ExternalGraphDatasetNode>? Nodes { get; init; }
    }

    private sealed class ExternalGraphDatasetNode
    {
        public string Id { get; init; } = string.Empty;

        public string Label { get; init; } = string.Empty;
    }
}
