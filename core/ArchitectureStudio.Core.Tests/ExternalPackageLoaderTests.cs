using System.Text;

namespace ArchitectureStudio.Core.Tests;

public sealed class ExternalPackageLoaderTests
{
    [Fact]
    public void Loader_discovers_required_sample_packs_and_their_contribution_points()
    {
        var packagesRoot = Path.Combine(AppContext.BaseDirectory, "Plugins", "Packs");

        var result = ExternalPackageLoader.Discover(packagesRoot);

        Assert.Contains(result.Packages, package => package.Id == "aws-architecture-pack");
        Assert.Contains(result.Packages, package => package.Id == "kafka-event-driven-pack");
        Assert.Contains(result.Packages, package => package.Id == "banking-compliance-pack");

        var aws = result.Packages.Single(package => package.Id == "aws-architecture-pack");
        var kafka = result.Packages.Single(package => package.Id == "kafka-event-driven-pack");
        var banking = result.Packages.Single(package => package.Id == "banking-compliance-pack");

        Assert.NotEmpty(aws.Contributions.StandardsPackages);
        Assert.NotEmpty(aws.Contributions.Templates);
        Assert.NotEmpty(aws.Contributions.GraphDatasets);
        Assert.NotEmpty(kafka.Contributions.GraphDatasets);
        Assert.NotEmpty(kafka.Contributions.Templates);
        Assert.NotEmpty(banking.Contributions.Regulations);
        Assert.NotEmpty(banking.Contributions.Controls);

        Assert.All(result.Statuses, static status => Assert.Equal(ExternalPackageStatusKind.Loaded, status.Status));
    }

    [Fact]
    public void Invalid_packages_fail_gracefully_without_breaking_valid_package_loading()
    {
        var root = Path.Combine(Path.GetTempPath(), "architecture-studio-plugin-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);

        try
        {
            CreatePackage(
                root,
                "valid-pack",
                """
{
  "id": "valid-pack",
  "version": "1.0.0",
  "schemaVersion": "1.0.0",
  "displayName": "Valid Pack",
  "contributions": {
    "standardsPackages": ["standards/valid.json"]
  }
}
""",
                ("standards/valid.json", """
{
  "id": "valid-pack",
  "version": "1.0.0",
  "sourcePath": "plugins/packs/valid-pack/standards/valid.json",
  "sourceTitle": "Valid Pack",
  "standards": []
}
"""));

            CreatePackage(
                root,
                "invalid-pack",
                """
{
  "id": "invalid-pack",
  "version": "1.0.0",
  "schemaVersion": "1.0.0",
  "displayName": "Invalid Pack",
  "contributions": {
    "templates": ["templates/missing.json"]
  }
}
""");

            var result = ExternalPackageLoader.Discover(root);

            Assert.Contains(result.Packages, package => package.Id == "valid-pack");
            Assert.DoesNotContain(result.Packages, package => package.Id == "invalid-pack");
            Assert.Contains(result.Statuses, status => status.PackageId == "valid-pack" && status.Status == ExternalPackageStatusKind.Loaded);
            Assert.Contains(result.Statuses, status => status.PackageId == "invalid-pack" && status.Status == ExternalPackageStatusKind.Invalid);
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    private static void CreatePackage(
        string root,
        string packageId,
        string manifestContent,
        params (string RelativePath, string Content)[] files)
    {
        var packageRoot = Path.Combine(root, packageId);
        Directory.CreateDirectory(packageRoot);

        File.WriteAllText(Path.Combine(packageRoot, "architecture-studio.package.json"), manifestContent, Encoding.UTF8);

        foreach (var (relativePath, content) in files)
        {
            var fullPath = Path.Combine(packageRoot, relativePath.Replace('/', Path.DirectorySeparatorChar));
            Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);
            File.WriteAllText(fullPath, content, Encoding.UTF8);
        }
    }
}
