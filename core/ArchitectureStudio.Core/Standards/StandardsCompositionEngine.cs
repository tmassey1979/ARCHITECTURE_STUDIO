namespace ArchitectureStudio.Core;

public sealed class StandardsCompositionEngine
{
    private static readonly IReadOnlyList<StandardsConsumerHint> DefaultConsumerHints =
    [
        new StandardsConsumerHint(
            StandardsConsumerWorkflow.Dashboard,
            "Render grouped standards summaries, rationale, and category coverage in the dashboard."),
        new StandardsConsumerHint(
            StandardsConsumerWorkflow.ProjectGenerator,
            "Feed deterministic standards selections into project, pipeline, and infrastructure generation."),
        new StandardsConsumerHint(
            StandardsConsumerWorkflow.ReportGenerator,
            "Explain selected standards, source packages, and reasons in generated reports."),
        new StandardsConsumerHint(
            StandardsConsumerWorkflow.AiInstructions,
            "Provide source-backed standards context for AGENTS.md and AI instruction outputs.")
    ];

    private readonly IReadOnlyDictionary<string, StandardsPackage> _packageByStandardId;
    private readonly IReadOnlyList<StandardsLibraryEntry> _entries;

    public StandardsCompositionEngine(StandardsCatalog catalog)
    {
        _entries = catalog.Entries;
        _packageByStandardId = catalog.Packages
            .SelectMany(package => package.Standards.Select(entry => new { package, entry.Definition.Id }))
            .ToDictionary(static item => item.Id, static item => item.package, StringComparer.OrdinalIgnoreCase);
    }

    public ComposedStandardsResult Compose(StandardsCompositionRequest request)
    {
        var explicitSelections = BuildSelectionIndex(EnumerateProjectSelections(request.ProjectSelection));
        var detectedTechnologies = BuildSelectionIndex(request.RepositoryCharacteristics?.DetectedTechnologies);
        var detectedTags = BuildSelectionIndex(request.RepositoryCharacteristics?.DetectedTags);
        var detectedCategories = new HashSet<StandardCategory>(request.RepositoryCharacteristics?.DetectedCategories ?? []);

        var selected = new List<ComposedStandard>();

        foreach (var entry in _entries)
        {
            var reasons = new List<string>();

            if (entry.Definition.Category == StandardCategory.Principles)
            {
                reasons.Add($"Baseline principle: {entry.Definition.Title}");
            }

            AddSelectionReasons(reasons, explicitSelections, entry, "Explicit selection");
            AddSelectionReasons(reasons, detectedTechnologies, entry, "Detected technology");
            AddTagReasons(reasons, detectedTags, entry);
            AddCategoryReasons(reasons, detectedCategories, entry);

            if (reasons.Count == 0)
            {
                continue;
            }

            var package = _packageByStandardId[entry.Definition.Id];
            selected.Add(
                new ComposedStandard(
                    Definition: entry.Definition,
                    Source: new StandardSourceMetadata(
                        PackageId: package.Id,
                        PackageVersion: package.Version,
                        SourcePath: package.SourcePath,
                        SourceTitle: entry.SourceTitle),
                    SelectionReasons: reasons
                        .Distinct(StringComparer.Ordinal)
                        .OrderBy(static reason => reason, StringComparer.Ordinal)
                        .ToArray()));
        }

        var ordered = selected
            .OrderBy(static standard => standard.Definition.Category)
            .ThenBy(static standard => standard.Definition.Title, StringComparer.Ordinal)
            .ThenBy(static standard => standard.Definition.Id, StringComparer.Ordinal)
            .ToArray();

        return new ComposedStandardsResult(
            Standards: ordered,
            ConsumerHints: DefaultConsumerHints);
    }

    private static Dictionary<string, string> BuildSelectionIndex(IEnumerable<string>? values)
    {
        var index = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        if (values is null)
        {
            return index;
        }

        foreach (var value in values)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                continue;
            }

            var normalized = Normalize(value);
            if (!index.ContainsKey(normalized))
            {
                index.Add(normalized, value.Trim());
            }
        }

        return index;
    }

    private static IEnumerable<string> EnumerateProjectSelections(StandardsProjectSelection? selection)
    {
        if (selection is null)
        {
            return [];
        }

        return
        [
            selection.Frontend,
            selection.Backend,
            selection.ArchitecturePattern,
            .. selection.CiCd,
            .. selection.Infrastructure,
            .. selection.AdditionalSelections
        ];
    }

    private static void AddSelectionReasons(
        ICollection<string> reasons,
        IReadOnlyDictionary<string, string> index,
        StandardsLibraryEntry entry,
        string prefix)
    {
        var keys = entry.AppliesToSelections
            .Prepend(entry.Definition.Title)
            .Prepend(entry.Definition.Id);

        foreach (var key in keys.Select(static value => Normalize(value)))
        {
            if (index.TryGetValue(key, out var original))
            {
                reasons.Add($"{prefix}: {original}");
            }
        }
    }

    private static void AddTagReasons(
        ICollection<string> reasons,
        IReadOnlyDictionary<string, string> detectedTags,
        StandardsLibraryEntry entry)
    {
        foreach (var tag in entry.Definition.Tags.Concat(entry.AppliesToTags))
        {
            if (detectedTags.TryGetValue(Normalize(tag), out var original))
            {
                reasons.Add($"Detected tag: {original}");
            }
        }
    }

    private static void AddCategoryReasons(
        ICollection<string> reasons,
        IReadOnlySet<StandardCategory> detectedCategories,
        StandardsLibraryEntry entry)
    {
        if (detectedCategories.Contains(entry.Definition.Category))
        {
            reasons.Add($"Detected category: {entry.Definition.Category}");
        }

        foreach (var category in entry.AppliesToCategories)
        {
            if (detectedCategories.Contains(category))
            {
                reasons.Add($"Detected category: {category}");
            }
        }
    }

    private static string Normalize(string value)
    {
        var buffer = value
            .Trim()
            .ToLowerInvariant()
            .Replace("_", " ", StringComparison.Ordinal)
            .Replace("-", " ", StringComparison.Ordinal);

        return string.Join(' ', buffer.Split(' ', StringSplitOptions.RemoveEmptyEntries));
    }
}
