namespace ArchitectureStudio.Core.Tests;

public sealed class ComplianceLibraryTests
{
    [Fact]
    public void Compliance_catalog_contains_required_regulation_modules_with_consistent_schema()
    {
        var catalog = ComplianceCatalog.CreateDefault();
        var regulationIds = catalog.Regulations.Select(static regulation => regulation.Id).ToHashSet(StringComparer.OrdinalIgnoreCase);

        foreach (var regulationId in new[]
        {
            "gdpr",
            "ccpa",
            "coppa",
            "hipaa",
            "hitech",
            "sox",
            "pci-dss",
            "iso-27001",
            "nist-csf",
            "soc2",
            "tcpa",
            "can-spam",
            "pipeda"
        })
        {
            Assert.Contains(regulationId, regulationIds);
        }

        Assert.All(catalog.Regulations, static regulation =>
        {
            Assert.False(string.IsNullOrWhiteSpace(regulation.Id));
            Assert.False(string.IsNullOrWhiteSpace(regulation.Jurisdiction));
            Assert.NotEmpty(regulation.RequiredControls);
            Assert.NotEmpty(regulation.DataTypes);
        });
    }

    [Fact]
    public void Compliance_control_library_contains_required_named_controls_and_broader_taxonomy_scale()
    {
        var catalog = ComplianceCatalog.CreateDefault();
        var controlIds = catalog.ControlsById.Keys.ToHashSet(StringComparer.OrdinalIgnoreCase);

        foreach (var controlId in new[]
        {
            "encryption-at-rest",
            "audit-logging",
            "role-based-access-control",
            "secrets-management",
            "network-segmentation",
            "data-retention",
            "consent-management"
        })
        {
            Assert.Contains(controlId, controlIds);
        }

        Assert.True(catalog.ControlsById.Count >= 14, "Expected a broader control taxonomy in the seeded library.");
    }

    [Theory]
    [MemberData(nameof(RepresentativeEvaluationPaths))]
    public void Representative_evaluation_path_exists_for_each_regulation_family(
        string expectedRegulationId,
        ComplianceEvaluationRequest request)
    {
        var engine = new ComplianceEngine(ComplianceCatalog.CreateDefault());

        var result = engine.Evaluate(request);

        Assert.Contains(result.Summaries, summary => summary.RegulationId == expectedRegulationId);
    }

    public static IEnumerable<object[]> RepresentativeEvaluationPaths()
    {
        yield return ["gdpr", CreateRequest(systemCharacteristics: ["public-web"], personal: true)];
        yield return ["ccpa", CreateRequest(systemCharacteristics: ["consumer-app"], personal: true)];
        yield return ["coppa", CreateRequest(systemCharacteristics: ["kids-app"], childData: true)];
        yield return ["hipaa", CreateRequest(systemCharacteristics: ["patient-portal"], health: true)];
        yield return ["hitech", CreateRequest(systemCharacteristics: ["health-integration"], health: true)];
        yield return ["sox", CreateRequest(systemCharacteristics: ["public-company"], financial: true)];
        yield return ["pci-dss", CreateRequest(systemCharacteristics: ["payments"], financial: true)];
        yield return ["iso-27001", CreateRequest(systemCharacteristics: ["enterprise-platform"])];
        yield return ["nist-csf", CreateRequest(systemCharacteristics: ["critical-infrastructure"])];
        yield return ["soc2", CreateRequest(systemCharacteristics: ["saas"], detectedTechnologyIds: ["github-actions"])];
        yield return ["tcpa", CreateRequest(systemCharacteristics: ["sms-marketing"])];
        yield return ["can-spam", CreateRequest(systemCharacteristics: ["email-marketing"])];
        yield return ["pipeda", CreateRequest(systemCharacteristics: ["canada-market"], personal: true)];
    }

    private static ComplianceEvaluationRequest CreateRequest(
        IReadOnlyList<string>? systemCharacteristics = null,
        IReadOnlyList<string>? detectedTechnologyIds = null,
        bool personal = false,
        bool financial = false,
        bool health = false,
        bool childData = false)
    {
        var signals = (detectedTechnologyIds ?? [])
            .Select(id => new RepositorySignal(
                Id: id,
                Label: id,
                Category: RepositorySignalCategory.CiCd,
                Confidence: 0.99,
                Evidence: [$"Detected {id}."],
                AffectedPaths: [$"signals/{id}.txt"]))
            .ToArray();

        var sensitiveData = new List<SensitiveDataClassification>();
        if (personal)
        {
            sensitiveData.Add(new SensitiveDataClassification(SensitiveDataCategory.Personal, 0.9, ["Personal data"], ["data/personal.json"]));
        }

        if (financial)
        {
            sensitiveData.Add(new SensitiveDataClassification(SensitiveDataCategory.Financial, 0.9, ["Financial data"], ["data/financial.json"]));
        }

        if (health)
        {
            sensitiveData.Add(new SensitiveDataClassification(SensitiveDataCategory.Health, 0.9, ["Health data"], ["data/health.json"]));
        }

        if (childData)
        {
            sensitiveData.Add(new SensitiveDataClassification(SensitiveDataCategory.ChildData, 0.9, ["Child data"], ["data/child.json"]));
        }

        return new ComplianceEvaluationRequest(
            SystemCharacteristics: systemCharacteristics ?? [],
            RepositoryAnalysis: new RepositoryAnalysisResult(signals, sensitiveData),
            ImplementedControlIds: ["audit-logging", "access-control", "role-based-access-control", "security-testing"]);
    }
}
