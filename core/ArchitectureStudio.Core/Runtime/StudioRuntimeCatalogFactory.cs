namespace ArchitectureStudio.Core;

internal sealed record StudioRuntimeCatalogs(
    StandardsCatalog StandardsCatalog,
    TechnologyGraphCatalog TechnologyGraphCatalog,
    ComplianceCatalog ComplianceCatalog,
    ProjectTemplateCatalog ProjectTemplateCatalog,
    ExternalPackageDiscoveryResult ExternalPackages);

internal static class StudioRuntimeCatalogFactory
{
    private const string RelativePackagesDirectory = "Plugins/Packs";

    public static StudioRuntimeCatalogs CreateDefault()
    {
        var discovery = ExternalPackageLoader.Discover(Path.Combine(AppContext.BaseDirectory, RelativePackagesDirectory));
        var standardsCatalog = StandardsCatalog.CreateBuiltIn();
        var technologyGraphCatalog = TechnologyGraphCatalog.CreateBuiltIn();
        var complianceCatalog = ComplianceCatalog.CreateBuiltIn();
        var projectTemplateCatalog = ProjectTemplateCatalog.CreateBuiltIn();

        foreach (var package in discovery.Packages)
        {
            standardsCatalog = standardsCatalog.WithExternalPackage(package);
            technologyGraphCatalog = technologyGraphCatalog.WithExternalPackage(package);
            complianceCatalog = complianceCatalog.WithExternalPackage(package);
            projectTemplateCatalog = projectTemplateCatalog.WithExternalPackage(package);
        }

        return new StudioRuntimeCatalogs(
            standardsCatalog,
            technologyGraphCatalog,
            complianceCatalog,
            projectTemplateCatalog,
            discovery);
    }
}
