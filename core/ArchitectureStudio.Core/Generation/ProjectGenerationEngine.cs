namespace ArchitectureStudio.Core;

public sealed class ProjectGenerationEngine
{
    private readonly ProjectTemplateCatalog _catalog;

    public ProjectGenerationEngine(ProjectTemplateCatalog catalog)
    {
        _catalog = catalog;
    }

    public ProjectGenerationResult Generate(ProjectSelectionProfile selection)
    {
        var selectedTemplates = _catalog.Templates
            .Where(template => MatchesSelection(template, selection))
            .OrderBy(static template => template.Id, StringComparer.Ordinal)
            .ToArray();

        var files = selectedTemplates
            .SelectMany(template => template.Files.Select(file => CreateFile(file, selection)))
            .GroupBy(static file => file.RelativePath, StringComparer.OrdinalIgnoreCase)
            .Select(static group => group.First())
            .OrderBy(static file => file.RelativePath, StringComparer.Ordinal)
            .ThenBy(static file => file.Title, StringComparer.Ordinal)
            .ToArray();

        var generatedArtifacts = files
            .Select(file => new GeneratedArtifact(
                Id: CreateArtifactId(file.RelativePath),
                Title: file.Title,
                Kind: file.Kind,
                RelativePath: file.RelativePath))
            .OrderBy(static artifact => artifact.RelativePath, StringComparer.Ordinal)
            .ThenBy(static artifact => artifact.Id, StringComparer.Ordinal)
            .ToArray();

        return new ProjectGenerationResult(
            TemplateIds: selectedTemplates.Select(static template => template.Id).ToArray(),
            GeneratedArtifacts: generatedArtifacts,
            Files: files);
    }

    private static bool MatchesSelection(ProjectTemplateDefinition template, ProjectSelectionProfile selection)
    {
        return template.SelectionKey switch
        {
            "always" => true,
            "frontend" => MatchesValue(template.SelectionValues, selection.Frontend),
            "backend" => MatchesValue(template.SelectionValues, selection.Backend),
            "architecturePattern" => MatchesValue(template.SelectionValues, selection.ArchitecturePattern),
            "ciCd" => MatchesAny(template.SelectionValues, selection.CiCd),
            "infrastructure" => MatchesAny(template.SelectionValues, selection.Infrastructure),
            "complianceTargets" => MatchesAny(template.SelectionValues, selection.ComplianceTargets),
            _ => false
        };
    }

    private static bool MatchesValue(IReadOnlyList<string> values, string actualValue)
    {
        return values.Any(value => string.Equals(value, actualValue, StringComparison.OrdinalIgnoreCase));
    }

    private static bool MatchesAny(IReadOnlyList<string> values, IReadOnlyList<string> actualValues)
    {
        return actualValues.Any(actualValue => MatchesValue(values, actualValue));
    }

    private static ProjectGeneratedFile CreateFile(ProjectTemplateFile templateFile, ProjectSelectionProfile selection)
    {
        return new ProjectGeneratedFile(
            RelativePath: Render(templateFile.RelativePath, selection),
            Title: Render(templateFile.Title, selection),
            Kind: templateFile.ArtifactKind,
            Content: Render(templateFile.Content, selection));
    }

    private static string Render(string template, ProjectSelectionProfile selection)
    {
        return template
            .Replace("{{frontend}}", selection.Frontend, StringComparison.Ordinal)
            .Replace("{{backend}}", selection.Backend, StringComparison.Ordinal)
            .Replace("{{architecturePattern}}", selection.ArchitecturePattern, StringComparison.Ordinal)
            .Replace("{{ciCd}}", string.Join(", ", selection.CiCd), StringComparison.Ordinal)
            .Replace("{{infrastructure}}", string.Join(", ", selection.Infrastructure), StringComparison.Ordinal)
            .Replace("{{complianceTargets}}", string.Join(", ", selection.ComplianceTargets), StringComparison.Ordinal);
    }

    private static string CreateArtifactId(string relativePath)
    {
        return relativePath
            .Replace('\\', '-')
            .Replace('/', '-')
            .Replace('.', '-')
            .Trim('-')
            .ToLowerInvariant();
    }
}
