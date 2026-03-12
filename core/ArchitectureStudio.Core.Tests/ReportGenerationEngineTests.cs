namespace ArchitectureStudio.Core.Tests;

public sealed class ReportGenerationEngineTests
{
    [Fact]
    public void Report_generation_creates_required_export_formats_named_docs_and_a_pdf_artifact()
    {
        var engine = new ReportGenerationEngine();
        var request = new ReportGenerationRequest(
            ProjectName: "Architecture Studio",
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

        foreach (var requiredPath in new[]
        {
            "reports/architecture-report.md",
            "reports/compliance-report.json",
            "reports/findings.sarif",
            "reports/engineering-playbook.md",
            "reports/security-policy.md",
            "reports/incident-response.md",
            "reports/architecture.md",
            "reports/architecture-report.pdf"
        })
        {
            Assert.Contains(requiredPath, paths);
        }

        Assert.Contains(result.ReportArtifacts, artifact => artifact.Format == ArtifactFormat.Markdown);
        Assert.Contains(result.ReportArtifacts, artifact => artifact.Format == ArtifactFormat.Pdf);
        Assert.Contains(result.ReportArtifacts, artifact => artifact.Format == ArtifactFormat.Json);
        Assert.Contains(result.ReportArtifacts, artifact => artifact.Format == ArtifactFormat.Sarif);
        Assert.False(result.PdfFallbackUsed);

        var jsonReport = result.Files.Single(file => file.RelativePath == "reports/compliance-report.json").Content;
        var sarifReport = result.Files.Single(file => file.RelativePath == "reports/findings.sarif").Content;
        var pdfReport = result.Files.Single(file => file.RelativePath == "reports/architecture-report.pdf").Content;

        Assert.Contains("\"scorePercentage\": 72", jsonReport, StringComparison.Ordinal);
        Assert.Contains("\"remediation\"", jsonReport, StringComparison.Ordinal);
        Assert.Contains("\"evidence\"", jsonReport, StringComparison.Ordinal);
        Assert.Contains("\"level\": \"warning\"", sarifReport, StringComparison.Ordinal);
        Assert.Contains("Implement audit logging", sarifReport, StringComparison.Ordinal);
        Assert.Contains("%PDF-1.4", pdfReport, StringComparison.Ordinal);
        Assert.Contains("Architecture Studio Architecture Report", pdfReport, StringComparison.Ordinal);
        Assert.Contains("HIPAA: 72%", pdfReport, StringComparison.Ordinal);
        Assert.Contains("HIPAA missing control: Audit Logging", pdfReport, StringComparison.Ordinal);
        Assert.Contains("src/Web/appsettings.json", pdfReport, StringComparison.Ordinal);
    }

    [Fact]
    public void Report_generation_is_deterministic_for_the_same_input()
    {
        var engine = new ReportGenerationEngine();
        var request = new ReportGenerationRequest(
            ProjectName: "Architecture Studio",
            ComplianceSummaries: [],
            Findings: []);

        var first = ContractJson.Serialize(engine.Generate(request));
        var second = ContractJson.Serialize(engine.Generate(request));

        Assert.Equal(first, second);
    }
}
