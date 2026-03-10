namespace ArchitectureStudio.Core.Tests;

public class SharedContractsTests
{
    [Fact]
    public void Severity_and_risk_levels_are_the_canonical_four_levels()
    {
        Assert.Equal(["Critical", "High", "Medium", "Low"], Enum.GetNames<SeverityLevel>());
        Assert.Equal(["Critical", "High", "Medium", "Low"], Enum.GetNames<RiskLevel>());
    }

    [Fact]
    public void Project_selection_profile_validation_accepts_a_complete_selection()
    {
        var profile = new ProjectSelectionProfile(
            Frontend: "react",
            Backend: "aspnet-core",
            ArchitecturePattern: "clean-architecture",
            CiCd: ["github-actions"],
            Infrastructure: ["docker", "kubernetes"],
            ComplianceTargets: ["hipaa"]);

        var validation = ContractValidation.ValidateProjectSelection(profile);

        Assert.True(validation.IsValid);
        Assert.Empty(validation.Errors);
    }

    [Fact]
    public void Project_selection_profile_validation_rejects_missing_required_fields()
    {
        var profile = new ProjectSelectionProfile(
            Frontend: "",
            Backend: "",
            ArchitecturePattern: "",
            CiCd: [],
            Infrastructure: [],
            ComplianceTargets: []);

        var validation = ContractValidation.ValidateProjectSelection(profile);

        Assert.False(validation.IsValid);
        Assert.Contains(validation.Errors, error => error.Contains("Frontend", StringComparison.Ordinal));
        Assert.Contains(validation.Errors, error => error.Contains("Backend", StringComparison.Ordinal));
        Assert.Contains(validation.Errors, error => error.Contains("ArchitecturePattern", StringComparison.Ordinal));
    }

    [Fact]
    public void Shared_contract_payload_round_trips_through_json_for_extension_boundaries()
    {
        var payload = new SharedContractPayload(
            Standards:
            [
                new StandardDefinition(
                    Id: "solid",
                    Title: "SOLID",
                    Category: StandardCategory.Principles,
                    Summary: "Object-oriented design principles.",
                    Tags: ["maintainability", "design"])
            ],
            Regulations:
            [
                new RegulationDefinition(
                    Id: "hipaa",
                    Category: RegulationCategory.Healthcare,
                    Jurisdiction: "US",
                    RequiredControls: ["encryption_at_rest", "audit_logging"],
                    DataTypes: ["protected_health_information"])
            ],
            Controls:
            [
                new ControlDefinition(
                    Id: "audit_logging",
                    Title: "Audit Logging",
                    Summary: "Track security-relevant access and changes.")
            ],
            GraphNodes:
            [
                new GraphNodeDefinition(
                    Id: "aspnet-core",
                    Label: "ASP.NET Core",
                    Category: GraphNodeCategory.Framework)
            ],
            GraphEdges:
            [
                new GraphEdgeDefinition(
                    SourceId: "aspnet-core",
                    TargetId: "cqrs",
                    Relationship: GraphRelationship.RecommendedWith)
            ],
            Findings:
            [
                new FindingDefinition(
                    Id: "missing-audit-logging",
                    Title: "Missing audit logging",
                    Summary: "Audit logging is required for HIPAA.",
                    Severity: SeverityLevel.High,
                    Risk: RiskLevel.High,
                    Remediation: new RemediationDefinition(
                        Title: "Add audit logging",
                        Summary: "Implement audit logging at the API and data access layers."))
            ],
            Reports:
            [
                new ReportArtifact(
                    Id: "compliance-summary",
                    Title: "Compliance Summary",
                    Format: ArtifactFormat.Markdown,
                    RelativePath: "reports/compliance-summary.md")
            ],
            GeneratedArtifacts:
            [
                new GeneratedArtifact(
                    Id: "agents-md",
                    Title: "AGENTS.md",
                    Kind: GeneratedArtifactKind.Documentation,
                    RelativePath: "docs/AGENTS.md")
            ],
            ProjectSelection: new ProjectSelectionProfile(
                Frontend: "react",
                Backend: "aspnet-core",
                ArchitecturePattern: "clean-architecture",
                CiCd: ["github-actions"],
                Infrastructure: ["docker"],
                ComplianceTargets: ["hipaa"]));

        var json = ContractJson.Serialize(payload);
        var roundTrip = ContractJson.Deserialize<SharedContractPayload>(json);

        Assert.NotNull(roundTrip);
        Assert.Equal("solid", roundTrip.Standards.Single().Id);
        Assert.Equal(SeverityLevel.High, roundTrip.Findings.Single().Severity);
        Assert.Equal(ArtifactFormat.Markdown, roundTrip.Reports.Single().Format);
    }
}
