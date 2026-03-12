namespace ArchitectureStudio.Core.Tests;

public sealed class AiInstructionGenerationEngineTests
{
    [Fact]
    public void Ai_instruction_generation_creates_agents_and_prompt_files_from_project_profile_context()
    {
        var engine = new AiInstructionGenerationEngine();
        var request = new AiInstructionGenerationRequest(
            ProjectName: "Architecture Studio",
            TargetKind: AiInstructionTargetKind.GeneratedProject,
            ProjectSelection: new ProjectSelectionProfile(
                Frontend: "react",
                Backend: "aspnet-core",
                ArchitecturePattern: "clean-architecture",
                CiCd: ["github-actions", "jenkins"],
                Infrastructure: ["docker", "kubernetes"],
                ComplianceTargets: ["hipaa", "soc2"]),
            Standards:
            [
                new StandardDefinition(
                    Id: "std-layering",
                    Title: "Service Layering",
                    Category: StandardCategory.Architecture,
                    Summary: "Keep application, domain, and infrastructure boundaries explicit.",
                    Tags: ["architecture", "boundaries"]),
                new StandardDefinition(
                    Id: "std-tdd",
                    Title: "Test Driven Delivery",
                    Category: StandardCategory.Testing,
                    Summary: "Drive implementation with failing automated tests first.",
                    Tags: ["tdd", "quality"])
            ],
            ComplianceSummaries:
            [
                new ComplianceSummary(
                    RegulationId: "hipaa",
                    RegulationTitle: "HIPAA",
                    ScorePercentage: 72,
                    CoveredControls: 5,
                    TotalControls: 7)
            ],
            Findings:
            [
                new FindingDefinition(
                    Id: "missing-control-hipaa-audit-logging",
                    Title: "HIPAA missing control: Audit Logging",
                    Summary: "Audit logging is required for HIPAA and is not fully covered.",
                    Severity: SeverityLevel.High,
                    Risk: RiskLevel.High,
                    Remediation: new RemediationDefinition(
                        Title: "Implement audit logging",
                        Summary: "Add auditable tracking for protected data access."),
                    Evidence: ["src/Web/appsettings.json"])
            ]);

        var result = engine.Generate(request);
        var paths = result.Files.Select(static file => file.RelativePath).ToHashSet(StringComparer.OrdinalIgnoreCase);

        Assert.Contains("AGENTS.md", paths);
        Assert.Contains("docs/ai-instructions.md", paths);

        var agentsFile = result.Files.Single(file => file.RelativePath == "AGENTS.md").Content;
        Assert.Contains("## Architecture Rules", agentsFile, StringComparison.Ordinal);
        Assert.Contains("## Coding Standards", agentsFile, StringComparison.Ordinal);
        Assert.Contains("## DevOps Rules", agentsFile, StringComparison.Ordinal);
        Assert.Contains("## Compliance Requirements", agentsFile, StringComparison.Ordinal);
        Assert.Contains("react", agentsFile, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("aspnet-core", agentsFile, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("clean-architecture", agentsFile, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Service Layering", agentsFile, StringComparison.Ordinal);
        Assert.Contains("Test Driven Delivery", agentsFile, StringComparison.Ordinal);
        Assert.Contains("HIPAA 72%", agentsFile, StringComparison.Ordinal);
        Assert.Contains("Implement audit logging", agentsFile, StringComparison.Ordinal);

        Assert.Contains(result.GeneratedArtifacts, artifact => artifact.RelativePath == "AGENTS.md" && artifact.Kind == GeneratedArtifactKind.AiInstructions);
    }

    [Fact]
    public void Ai_instruction_generation_is_deterministic_for_the_same_input()
    {
        var engine = new AiInstructionGenerationEngine();
        var request = new AiInstructionGenerationRequest(
            ProjectName: "Architecture Studio",
            TargetKind: AiInstructionTargetKind.AnalyzedRepository,
            ProjectSelection: new ProjectSelectionProfile(
                Frontend: "angular",
                Backend: "spring-boot",
                ArchitecturePattern: "microservices",
                CiCd: ["gitlab-ci"],
                Infrastructure: ["helm"],
                ComplianceTargets: ["gdpr"]),
            Standards: [],
            ComplianceSummaries: [],
            Findings: []);

        var first = ContractJson.Serialize(engine.Generate(request));
        var second = ContractJson.Serialize(engine.Generate(request));

        Assert.Equal(first, second);
    }
}
