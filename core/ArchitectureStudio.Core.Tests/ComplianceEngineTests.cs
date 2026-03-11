namespace ArchitectureStudio.Core.Tests;

public sealed class ComplianceEngineTests
{
    [Fact]
    public void Compliance_engine_detects_applicable_regulations_scores_controls_and_emits_missing_control_findings()
    {
        var engine = new ComplianceEngine(ComplianceCatalog.CreateDefault());
        var request = new ComplianceEvaluationRequest(
            SystemCharacteristics: ["patient-portal", "kids-app"],
            RepositoryAnalysis: new RepositoryAnalysisResult(
                Signals:
                [
                    new RepositorySignal(
                        Id: "github-actions",
                        Label: "GitHub Actions",
                        Category: RepositorySignalCategory.CiCd,
                        Confidence: 0.99,
                        Evidence: ["Detected GitHub Actions workflow file."],
                        AffectedPaths: [".github/workflows/ci.yml"])
                ],
                SensitiveData:
                [
                    new SensitiveDataClassification(
                        Category: SensitiveDataCategory.Health,
                        Confidence: 0.94,
                        Evidence: ["Health data indicator matched in appsettings.json."],
                        AffectedPaths: ["src/Web/appsettings.json"]),
                    new SensitiveDataClassification(
                        Category: SensitiveDataCategory.Personal,
                        Confidence: 0.90,
                        Evidence: ["Personal data indicator matched in appsettings.json."],
                        AffectedPaths: ["src/Web/appsettings.json"]),
                    new SensitiveDataClassification(
                        Category: SensitiveDataCategory.ChildData,
                        Confidence: 0.93,
                        Evidence: ["Child data indicator matched in appsettings.json."],
                        AffectedPaths: ["src/Web/appsettings.json"])
                ]),
            ImplementedControlIds: ["audit-logging", "access-control"]);

        var result = engine.Evaluate(request);

        Assert.Contains(result.Summaries, summary => summary.RegulationId == "hipaa" && summary.ScorePercentage == 50);
        Assert.Contains(result.Summaries, summary => summary.RegulationId == "gdpr" && summary.ScorePercentage == 33);
        Assert.Contains(result.Summaries, summary => summary.RegulationId == "coppa" && summary.ScorePercentage == 50);
        Assert.Contains(result.Summaries, summary => summary.RegulationId == "soc2" && summary.ScorePercentage == 67);
        Assert.Contains(result.Findings, finding => finding.Id == "missing-control-hipaa-encryption-at-rest");
        Assert.Contains(result.Findings, finding => finding.Id == "missing-control-gdpr-consent-management");
        Assert.All(result.Findings, static finding => Assert.NotNull(finding.Remediation));
    }

    [Fact]
    public void Compliance_engine_is_repeatable_for_the_same_input()
    {
        var engine = new ComplianceEngine(ComplianceCatalog.CreateDefault());
        var request = new ComplianceEvaluationRequest(
            SystemCharacteristics: ["saas"],
            RepositoryAnalysis: new RepositoryAnalysisResult(
                Signals:
                [
                    new RepositorySignal(
                        Id: "jenkins",
                        Label: "Jenkins",
                        Category: RepositorySignalCategory.CiCd,
                        Confidence: 0.99,
                        Evidence: ["Detected Jenkins pipeline file."],
                        AffectedPaths: ["Jenkinsfile"])
                ],
                SensitiveData:
                [
                    new SensitiveDataClassification(
                        Category: SensitiveDataCategory.Financial,
                        Confidence: 0.92,
                        Evidence: ["Financial data indicator matched in billing.json."],
                        AffectedPaths: ["config/billing.json"])
                ]),
            ImplementedControlIds: ["audit-logging", "access-control", "security-testing"]);

        var first = ContractJson.Serialize(engine.Evaluate(request));
        var second = ContractJson.Serialize(engine.Evaluate(request));

        Assert.Equal(first, second);
    }
}
