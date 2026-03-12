namespace ArchitectureStudio.Core.Tests;

public sealed class StudioWorkspaceOrchestratorTests
{
    [Fact]
    public void Workspace_orchestrator_infers_project_selection_and_drives_cross_engine_outputs()
    {
        var workspacePath = Path.Combine(AppContext.BaseDirectory, "Fixtures", "SampleWorkspaces", "fintech-platform");
        Assert.True(Directory.Exists(workspacePath), $"Workspace fixture '{workspacePath}' was not found.");

        var orchestrator = StudioWorkspaceOrchestrator.CreateDefault();

        var selection = orchestrator.InferProjectSelection(workspacePath);
        Assert.Equal("react", selection.Frontend);
        Assert.Equal("aspnet-core", selection.Backend);
        Assert.Equal("clean-architecture", selection.ArchitecturePattern);
        Assert.Contains("github-actions", selection.CiCd);
        Assert.Contains("docker", selection.Infrastructure);
        Assert.Contains("pci-dss", selection.ComplianceTargets);

        var standards = orchestrator.ComposeStandards(workspacePath);
        Assert.Contains(standards.Standards, standard => standard.Definition.Id == "react");
        Assert.Contains(standards.Standards, standard => standard.Definition.Id == "github-actions");
        Assert.Contains(standards.Standards, standard => standard.Definition.Id == "docker");

        var architecture = orchestrator.EvaluateArchitecture(workspacePath);
        Assert.Contains(architecture.TechnologyEvaluation.SelectedNodes, node => node.Id == "react");
        Assert.Contains(architecture.TechnologyEvaluation.SelectedNodes, node => node.Id == "aspnet-core");
        Assert.NotEmpty(architecture.TechnologyEvaluation.Recommendations);

        var compliance = orchestrator.ValidateRegulations(workspacePath);
        Assert.Contains(compliance.Summaries, summary => summary.RegulationId == "pci-dss");
        Assert.NotEmpty(compliance.Findings);

        var project = orchestrator.GenerateProject(workspacePath);
        Assert.Contains(project.TemplateIds, id => id == "frontend-react");
        Assert.Contains(project.TemplateIds, id => id == "backend-aspnet-core");
        Assert.Contains(project.TemplateIds, id => id == "pipeline-github-actions");
        Assert.Contains(project.TemplateIds, id => id == "compliance-pci-dss");

        var reports = orchestrator.GenerateReports(workspacePath);
        Assert.Contains(reports.ReportArtifacts, artifact => artifact.RelativePath == "reports/compliance-report.json");

        var aiRequest = orchestrator.BuildAiInstructionRequest(workspacePath);
        Assert.Equal(AiInstructionTargetKind.AnalyzedRepository, aiRequest.TargetKind);
        Assert.Contains("Fintech", aiRequest.ProjectName, StringComparison.Ordinal);
        Assert.NotEmpty(aiRequest.Standards);
        Assert.NotEmpty(aiRequest.ComplianceSummaries);

        var aiOutput = orchestrator.GenerateAiInstructions(workspacePath);
        Assert.Contains(aiOutput.GeneratedArtifacts, artifact => artifact.RelativePath == "AGENTS.md");
    }
}
