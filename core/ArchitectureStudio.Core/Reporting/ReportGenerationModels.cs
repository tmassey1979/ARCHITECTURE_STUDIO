namespace ArchitectureStudio.Core;

public sealed record ReportGenerationRequest(
    string ProjectName,
    IReadOnlyList<ComplianceSummary> ComplianceSummaries,
    IReadOnlyList<FindingDefinition> Findings,
    string ExportRoot = "reports");

public sealed record GeneratedReportFile(
    string RelativePath,
    ArtifactFormat Format,
    string Content);

public sealed record ReportGenerationResult(
    IReadOnlyList<ReportArtifact> ReportArtifacts,
    IReadOnlyList<GeneratedReportFile> Files,
    bool PdfFallbackUsed);
