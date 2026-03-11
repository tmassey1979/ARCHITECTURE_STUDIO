namespace ArchitectureStudio.Core;

public sealed class ComplianceEngine
{
    private readonly ComplianceCatalog _catalog;

    public ComplianceEngine(ComplianceCatalog catalog)
    {
        _catalog = catalog;
    }

    public ComplianceEvaluationResult Evaluate(ComplianceEvaluationRequest request)
    {
        var matchedSystemCharacteristics = request.SystemCharacteristics
            .Where(static value => !string.IsNullOrWhiteSpace(value))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        var matchedTechnologies = request.RepositoryAnalysis.Signals
            .Select(static signal => signal.Id)
            .Where(static value => !string.IsNullOrWhiteSpace(value))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        var matchedSensitiveDataCategories = request.RepositoryAnalysis.SensitiveData
            .Select(static item => item.Category)
            .ToHashSet();
        var implementedControls = request.ImplementedControlIds
            .Where(static value => !string.IsNullOrWhiteSpace(value))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var applicableRegulations = _catalog.Regulations
            .Where(regulation => IsApplicable(regulation, matchedSystemCharacteristics, matchedTechnologies, matchedSensitiveDataCategories))
            .OrderBy(static regulation => regulation.Title, StringComparer.Ordinal)
            .ThenBy(static regulation => regulation.Id, StringComparer.Ordinal)
            .ToArray();

        var summaries = new List<ComplianceSummary>();
        var findings = new List<FindingDefinition>();

        foreach (var regulation in applicableRegulations)
        {
            var requiredControls = regulation.RequiredControls
                .Where(static controlId => !string.IsNullOrWhiteSpace(controlId))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(static controlId => controlId, StringComparer.Ordinal)
                .ToArray();
            var coveredControls = requiredControls.Count(implementedControls.Contains);
            var totalControls = requiredControls.Length;
            var score = totalControls == 0
                ? 100
                : (int)Math.Round((double)coveredControls / totalControls * 100, MidpointRounding.AwayFromZero);

            summaries.Add(
                new ComplianceSummary(
                    RegulationId: regulation.Id,
                    RegulationTitle: regulation.Title,
                    ScorePercentage: score,
                    CoveredControls: coveredControls,
                    TotalControls: totalControls));

            var applicabilityEvidence = BuildApplicabilityEvidence(
                regulation.Applicability,
                matchedSystemCharacteristics,
                matchedTechnologies,
                matchedSensitiveDataCategories,
                score);

            foreach (var controlId in requiredControls.Where(controlId => !implementedControls.Contains(controlId)))
            {
                if (!_catalog.ControlsById.TryGetValue(controlId, out var control))
                {
                    continue;
                }

                findings.Add(
                    new FindingDefinition(
                        Id: $"missing-control-{regulation.Id}-{control.Id}",
                        Title: $"{regulation.Title} missing control: {control.Title}",
                        Summary: $"{control.Title} is required for {regulation.Title} and is not fully covered.",
                        Severity: score < 50 ? SeverityLevel.Critical : SeverityLevel.High,
                        Risk: score < 50 ? RiskLevel.Critical : RiskLevel.High,
                        Remediation: new RemediationDefinition(
                            Title: control.RemediationTitle,
                            Summary: control.RemediationSummary),
                        Evidence: applicabilityEvidence));
            }
        }

        return new ComplianceEvaluationResult(
            Summaries: summaries
                .OrderBy(static summary => summary.RegulationTitle, StringComparer.Ordinal)
                .ThenBy(static summary => summary.RegulationId, StringComparer.Ordinal)
                .ToArray(),
            Findings: findings
                .OrderBy(static finding => finding.Title, StringComparer.Ordinal)
                .ThenBy(static finding => finding.Id, StringComparer.Ordinal)
                .ToArray());
    }

    private static bool IsApplicable(
        ComplianceRegulationDefinition regulation,
        IReadOnlySet<string> systemCharacteristics,
        IReadOnlySet<string> technologies,
        IReadOnlySet<SensitiveDataCategory> sensitiveDataCategories)
    {
        return Matches(regulation.Applicability.SystemCharacteristics, systemCharacteristics)
            || Matches(regulation.Applicability.DetectedTechnologies, technologies)
            || regulation.DataTypes.Any(sensitiveDataCategories.Contains);
    }

    private static bool Matches(IReadOnlyList<string> expectedValues, IReadOnlySet<string> actualValues)
    {
        return expectedValues.Any(actualValues.Contains);
    }

    private static IReadOnlyList<string> BuildApplicabilityEvidence(
        ComplianceApplicability applicability,
        IReadOnlySet<string> systemCharacteristics,
        IReadOnlySet<string> technologies,
        IReadOnlySet<SensitiveDataCategory> sensitiveDataCategories,
        int score)
    {
        var evidence = new List<string>();

        var matchedSystemCharacteristics = applicability.SystemCharacteristics
            .Where(systemCharacteristics.Contains)
            .OrderBy(static value => value, StringComparer.Ordinal)
            .ToArray();
        if (matchedSystemCharacteristics.Length > 0)
        {
            evidence.Add($"Matched system characteristics: {string.Join(", ", matchedSystemCharacteristics)}.");
        }

        var matchedTechnologies = applicability.DetectedTechnologies
            .Where(technologies.Contains)
            .OrderBy(static value => value, StringComparer.Ordinal)
            .ToArray();
        if (matchedTechnologies.Length > 0)
        {
            evidence.Add($"Matched detected technologies: {string.Join(", ", matchedTechnologies)}.");
        }

        var matchedSensitiveData = applicability.SensitiveDataCategories
            .Where(sensitiveDataCategories.Contains)
            .Select(static category => category.ToString())
            .OrderBy(static value => value, StringComparer.Ordinal)
            .ToArray();
        if (matchedSensitiveData.Length > 0)
        {
            evidence.Add($"Matched sensitive data categories: {string.Join(", ", matchedSensitiveData)}.");
        }

        evidence.Add($"Current compliance score: {score}%.");
        return evidence;
    }
}
