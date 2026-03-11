namespace ArchitectureStudio.Core;

public sealed record StandardsLibraryEntry(
    StandardDefinition Definition,
    IReadOnlyList<StandardCategory> AppliesToCategories,
    IReadOnlyList<string> AppliesToSelections,
    IReadOnlyList<string> AppliesToTags,
    string SourceTitle);

public sealed record StandardsPackage(
    string Id,
    string Version,
    string SourcePath,
    IReadOnlyList<StandardsLibraryEntry> Standards);

public sealed record StandardsProjectSelection(
    string Frontend,
    string Backend,
    string ArchitecturePattern,
    IReadOnlyList<string> CiCd,
    IReadOnlyList<string> Infrastructure,
    IReadOnlyList<string> AdditionalSelections);

public sealed record RepositoryCharacteristics(
    IReadOnlyList<string> DetectedTechnologies,
    IReadOnlyList<string> DetectedTags,
    IReadOnlyList<StandardCategory> DetectedCategories);

public sealed record StandardsCompositionRequest(
    StandardsProjectSelection? ProjectSelection = null,
    RepositoryCharacteristics? RepositoryCharacteristics = null);

public sealed record StandardSourceMetadata(
    string PackageId,
    string PackageVersion,
    string SourcePath,
    string SourceTitle);

public sealed record ComposedStandard(
    StandardDefinition Definition,
    StandardSourceMetadata Source,
    IReadOnlyList<string> SelectionReasons);

public enum StandardsConsumerWorkflow
{
    Dashboard,
    ProjectGenerator,
    ReportGenerator,
    AiInstructions
}

public sealed record StandardsConsumerHint(
    StandardsConsumerWorkflow Workflow,
    string Usage);

public sealed record ComposedStandardsResult(
    IReadOnlyList<ComposedStandard> Standards,
    IReadOnlyList<StandardsConsumerHint> ConsumerHints)
{
    public IReadOnlyList<StandardDefinition> ToSharedStandards() =>
        Standards.Select(static standard => standard.Definition).ToArray();
}
