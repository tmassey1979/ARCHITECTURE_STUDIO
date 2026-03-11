namespace ArchitectureStudio.Core;

public sealed record ComplianceApplicability(
    IReadOnlyList<string> SystemCharacteristics,
    IReadOnlyList<string> DetectedTechnologies,
    IReadOnlyList<SensitiveDataCategory> SensitiveDataCategories);

public sealed record ComplianceControlDefinition(
    string Id,
    string Title,
    string Summary,
    string RemediationTitle,
    string RemediationSummary);

public sealed record ComplianceRegulationDefinition(
    string Id,
    string Title,
    RegulationCategory Category,
    string Jurisdiction,
    string Summary,
    IReadOnlyList<string> RequiredControls,
    ComplianceApplicability Applicability);

public sealed record ComplianceEvaluationRequest(
    IReadOnlyList<string> SystemCharacteristics,
    RepositoryAnalysisResult RepositoryAnalysis,
    IReadOnlyList<string> ImplementedControlIds);

public sealed record ComplianceEvaluationResult(
    IReadOnlyList<ComplianceSummary> Summaries,
    IReadOnlyList<FindingDefinition> Findings);
