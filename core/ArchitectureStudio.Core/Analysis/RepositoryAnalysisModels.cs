namespace ArchitectureStudio.Core;

public enum RepositorySignalCategory
{
    Language,
    Framework,
    ArchitecturePattern,
    Infrastructure,
    CiCd
}

public enum SensitiveDataCategory
{
    Personal,
    Financial,
    Health,
    ChildData
}

public sealed record RepositorySignal(
    string Id,
    string Label,
    RepositorySignalCategory Category,
    double Confidence,
    IReadOnlyList<string> Evidence,
    IReadOnlyList<string> AffectedPaths);

public sealed record SensitiveDataClassification(
    SensitiveDataCategory Category,
    double Confidence,
    IReadOnlyList<string> Evidence,
    IReadOnlyList<string> AffectedPaths);

public sealed record RepositoryAnalysisResult(
    IReadOnlyList<RepositorySignal> Signals,
    IReadOnlyList<SensitiveDataClassification> SensitiveData);
