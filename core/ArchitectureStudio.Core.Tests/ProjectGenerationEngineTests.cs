namespace ArchitectureStudio.Core.Tests;

public sealed class ProjectGenerationEngineTests
{
    [Fact]
    public void Template_catalog_contains_required_pipeline_and_variation_coverage()
    {
        var catalog = ProjectTemplateCatalog.CreateDefault();
        var templateIds = catalog.Templates.Select(static template => template.Id).ToHashSet(StringComparer.OrdinalIgnoreCase);

        foreach (var requiredId in new[]
        {
            "frontend-react",
            "frontend-angular",
            "backend-aspnet-core",
            "backend-spring-boot",
            "architecture-clean-architecture",
            "architecture-hexagonal-architecture",
            "architecture-microservices",
            "pipeline-github-actions",
            "pipeline-gitlab-ci",
            "pipeline-jenkins",
            "pipeline-azure-devops",
            "pipeline-circleci",
            "infra-docker",
            "infra-kubernetes",
            "compliance-hipaa",
            "compliance-gdpr",
            "compliance-soc2"
        })
        {
            Assert.Contains(requiredId, templateIds);
        }
    }

    [Fact]
    public void Generator_creates_deterministic_project_pipeline_and_infrastructure_artifacts_for_a_selection()
    {
        var engine = new ProjectGenerationEngine(ProjectTemplateCatalog.CreateDefault());
        var selection = new ProjectSelectionProfile(
            Frontend: "react",
            Backend: "aspnet-core",
            ArchitecturePattern: "clean-architecture",
            CiCd: ["github-actions", "circleci"],
            Infrastructure: ["docker", "kubernetes"],
            ComplianceTargets: ["hipaa", "soc2"]);

        var first = engine.Generate(selection);
        var second = engine.Generate(selection);

        Assert.Equal(
            ContractJson.Serialize(first),
            ContractJson.Serialize(second));

        var paths = first.Files.Select(static file => file.RelativePath).ToHashSet(StringComparer.OrdinalIgnoreCase);

        foreach (var requiredPath in new[]
        {
            "src/README.md",
            "services/README.md",
            "frontend/README.md",
            "infrastructure/README.md",
            "docs/architecture/clean-architecture.md",
            "docker/docker-compose.yml",
            "k8s/base/deployment.yaml",
            ".github/workflows/ci.yml",
            ".circleci/config.yml"
        })
        {
            Assert.Contains(requiredPath, paths);
        }

        Assert.Contains(first.GeneratedArtifacts, artifact => artifact.RelativePath == "docs/architecture/clean-architecture.md");
        Assert.Contains(first.TemplateIds, static id => id == "pipeline-github-actions");
        Assert.Contains(first.TemplateIds, static id => id == "pipeline-circleci");
    }
}
