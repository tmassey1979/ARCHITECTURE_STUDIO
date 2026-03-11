using System.Text.Json.Serialization;

namespace ArchitectureStudio.Core;

public sealed record ComplianceApplicability(
    [property: JsonPropertyName("system_characteristics")] IReadOnlyList<string> SystemCharacteristics,
    [property: JsonPropertyName("detected_technologies")] IReadOnlyList<string> DetectedTechnologies,
    [property: JsonPropertyName("sensitive_data_categories")] IReadOnlyList<SensitiveDataCategory> SensitiveDataCategories);

public sealed record ComplianceControlDefinition(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("title")] string Title,
    [property: JsonPropertyName("summary")] string Summary,
    [property: JsonPropertyName("remediation_title")] string RemediationTitle,
    [property: JsonPropertyName("remediation_summary")] string RemediationSummary);

public sealed record ComplianceRegulationDefinition(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("title")] string Title,
    [property: JsonPropertyName("category")] RegulationCategory Category,
    [property: JsonPropertyName("jurisdiction")] string Jurisdiction,
    [property: JsonPropertyName("summary")] string Summary,
    [property: JsonPropertyName("required_controls")] IReadOnlyList<string> RequiredControls,
    [property: JsonPropertyName("data_types")] IReadOnlyList<SensitiveDataCategory> DataTypes,
    [property: JsonPropertyName("applicability")] ComplianceApplicability Applicability);

public sealed record ComplianceEvaluationRequest(
    IReadOnlyList<string> SystemCharacteristics,
    RepositoryAnalysisResult RepositoryAnalysis,
    IReadOnlyList<string> ImplementedControlIds);

public sealed record ComplianceEvaluationResult(
    IReadOnlyList<ComplianceSummary> Summaries,
    IReadOnlyList<FindingDefinition> Findings);
