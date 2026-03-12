namespace ArchitectureStudio.Core.Tests;

public sealed class SmokeFixtureTests
{
    [Fact]
    public void Curated_fintech_fixture_supports_analysis_compliance_and_reporting_end_to_end()
    {
        var workspacePath = Path.Combine(AppContext.BaseDirectory, "Fixtures", "SampleWorkspaces", "fintech-platform");
        Assert.True(Directory.Exists(workspacePath), $"Smoke fixture workspace '{workspacePath}' was not found.");

        var analysis = new RepositoryAnalysisEngine().Analyze(workspacePath);

        Assert.Contains(analysis.Signals, signal => signal.Id == "aspnet-core");
        Assert.Contains(analysis.Signals, signal => signal.Id == "github-actions");
        Assert.Contains(analysis.SensitiveData, item => item.Category == SensitiveDataCategory.Personal);
        Assert.Contains(analysis.SensitiveData, item => item.Category == SensitiveDataCategory.Financial);

        var compliance = new ComplianceEngine(ComplianceCatalog.CreateDefault()).Evaluate(
            new ComplianceEvaluationRequest(
                SystemCharacteristics: ["payments"],
                RepositoryAnalysis: analysis,
                ImplementedControlIds: ["access-control", "audit-logging"]));

        Assert.Contains(compliance.Summaries, summary => summary.RegulationId == "pci-dss");
        Assert.NotEmpty(compliance.Findings);

        var reports = new ReportGenerationEngine().Generate(
            new ReportGenerationRequest(
                ProjectName: "Fintech Platform",
                ComplianceSummaries: compliance.Summaries,
                Findings: compliance.Findings));

        var complianceJson = reports.Files.Single(file => file.RelativePath == "reports/compliance-report.json").Content;
        Assert.Contains("\"projectName\": \"Fintech Platform\"", complianceJson, StringComparison.Ordinal);
        Assert.Contains("\"regulationId\": \"pci-dss\"", complianceJson, StringComparison.OrdinalIgnoreCase);
        Assert.Contains(reports.Files, file => file.RelativePath == "reports/findings.sarif");
    }
}
