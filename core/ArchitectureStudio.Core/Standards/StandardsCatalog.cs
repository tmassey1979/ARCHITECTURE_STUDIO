namespace ArchitectureStudio.Core;

public sealed class StandardsCatalog
{
    private const string DefaultSeedResourceName = "ArchitectureStudio.Core.Standards.Packages.architecture-studio.seed.json";

    private readonly IReadOnlyList<StandardsPackage> _packages;

    private StandardsCatalog(IReadOnlyList<StandardsPackage> packages)
    {
        _packages = packages
            .OrderBy(static package => package.Id, StringComparer.Ordinal)
            .ThenBy(static package => package.Version, StringComparer.Ordinal)
            .ToArray();
    }

    public IReadOnlyList<StandardsPackage> Packages => _packages;

    public IReadOnlyList<StandardsLibraryEntry> Entries =>
        _packages
            .SelectMany(static package => package.Standards)
            .OrderBy(static entry => entry.Definition.Category)
            .ThenBy(static entry => entry.Definition.Title, StringComparer.Ordinal)
            .ThenBy(static entry => entry.Definition.Id, StringComparer.Ordinal)
            .ToArray();

    public IReadOnlyList<StandardDefinition> Standards =>
        Entries.Select(static entry => entry.Definition).ToArray();

    public static StandardsCatalog CreateDefault()
    {
        return StudioRuntimeCatalogFactory.CreateDefault().StandardsCatalog;
    }

    internal static StandardsCatalog CreateBuiltIn()
    {
        var assembly = typeof(StandardsCatalog).Assembly;
        using var stream = assembly.GetManifestResourceStream(DefaultSeedResourceName)
            ?? throw new InvalidOperationException($"Embedded standards seed resource '{DefaultSeedResourceName}' was not found.");

        var package = StandardsJson.Deserialize<StandardsPackage>(stream);
        return new StandardsCatalog([package]);
    }

    public StandardsCatalog WithPackage(StandardsPackage package)
    {
        var packages = _packages
            .Where(existing => !string.Equals(existing.Id, package.Id, StringComparison.Ordinal))
            .Concat([package])
            .ToArray();

        return new StandardsCatalog(packages);
    }

    internal StandardsCatalog WithPackages(IEnumerable<StandardsPackage> packages)
    {
        var catalog = this;

        foreach (var package in packages)
        {
            catalog = catalog.WithPackage(package);
        }

        return catalog;
    }

    internal StandardsCatalog WithExternalPackage(ExternalPackage package)
    {
        var packages = package.Contributions.StandardsPackages
            .Select(static reference =>
            {
                using var stream = File.OpenRead(reference.FullPath);
                return StandardsJson.Deserialize<StandardsPackage>(stream);
            })
            .ToArray();

        return WithPackages(packages);
    }
}
