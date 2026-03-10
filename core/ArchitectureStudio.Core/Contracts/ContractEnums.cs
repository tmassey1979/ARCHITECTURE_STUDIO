namespace ArchitectureStudio.Core;

public enum SeverityLevel
{
    Critical,
    High,
    Medium,
    Low
}

public enum RiskLevel
{
    Critical,
    High,
    Medium,
    Low
}

public enum StandardCategory
{
    Principles,
    Architecture,
    Frontend,
    Backend,
    DevOps,
    Testing,
    Security,
    Observability,
    Process
}

public enum RegulationCategory
{
    Privacy,
    Healthcare,
    Financial,
    Security,
    Communications
}

public enum GraphNodeCategory
{
    Technology,
    Framework,
    ArchitecturePattern,
    Regulation,
    Control
}

public enum GraphRelationship
{
    Requires,
    Conflicts,
    PairsWith,
    RecommendedWith
}

public enum ArtifactFormat
{
    Markdown,
    Pdf,
    Json,
    Sarif
}

public enum GeneratedArtifactKind
{
    ProjectScaffold,
    Pipeline,
    Infrastructure,
    Documentation,
    Report,
    AiInstructions
}
