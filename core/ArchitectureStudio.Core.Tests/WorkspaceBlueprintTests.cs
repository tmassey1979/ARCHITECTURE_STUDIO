namespace ArchitectureStudio.Core.Tests;

public class WorkspaceBlueprintTests
{
    [Fact]
    public void RequiredTopLevelDirectories_include_expected_bootstrap_folders()
    {
        var expected = new[]
        {
            "src",
            "core",
            "standards",
            "compliance",
            "reports"
        };

        foreach (var directory in expected)
        {
            Assert.Contains(directory, WorkspaceBlueprint.RequiredTopLevelDirectories);
        }
    }
}
