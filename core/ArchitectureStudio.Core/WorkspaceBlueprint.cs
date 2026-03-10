namespace ArchitectureStudio.Core;

public static class WorkspaceBlueprint
{
    public static IReadOnlyList<string> RequiredTopLevelDirectories { get; } =
    [
        "analysis",
        "changelog",
        "compliance",
        "core",
        "docs",
        "generators",
        "graph",
        "reports",
        "src",
        "standards",
        "templates",
        "ui"
    ];
}
