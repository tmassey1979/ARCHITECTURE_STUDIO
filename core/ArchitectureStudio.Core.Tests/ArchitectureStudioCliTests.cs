using System.Diagnostics;
using ArchitectureStudio.Cli;

namespace ArchitectureStudio.Core.Tests;

public sealed class ArchitectureStudioCliTests
{
    [Fact]
    public async Task Cli_help_lists_the_supported_workflow_commands()
    {
        var output = new StringWriter();
        var error = new StringWriter();
        var app = ArchitectureStudioCliApp.CreateDefault(output, error, new StringReader(string.Empty));

        var exitCode = await app.InvokeAsync(["--help"]);

        Assert.Equal(0, exitCode);
        Assert.Empty(error.ToString());

        var helpText = output.ToString();
        Assert.Contains("analyze-repository", helpText);
        Assert.Contains("compose-standards", helpText);
        Assert.Contains("evaluate-architecture", helpText);
        Assert.Contains("validate-regulations", helpText);
        Assert.Contains("infer-project-selection", helpText);
        Assert.Contains("generate-project", helpText);
        Assert.Contains("generate-reports", helpText);
        Assert.Contains("build-ai-instruction-request", helpText);
        Assert.Contains("generate-ai-instructions", helpText);
    }

    [Fact]
    public async Task Cli_workflow_commands_emit_stable_json_for_automation()
    {
        var workspacePath = Path.Combine(AppContext.BaseDirectory, "Fixtures", "SampleWorkspaces", "fintech-platform");
        Assert.True(Directory.Exists(workspacePath), $"Workspace fixture '{workspacePath}' was not found.");

        var orchestrator = StudioWorkspaceOrchestrator.CreateDefault();
        var projectSelection = orchestrator.InferProjectSelection(workspacePath);
        var aiInstructionRequest = orchestrator.BuildAiInstructionRequest(workspacePath);

        await AssertWorkspaceCommandAsync<RepositoryAnalysisResult>(
            ["analyze-repository", "--workspace", workspacePath],
            String.Empty);
        await AssertWorkspaceCommandAsync<ComposedStandardsResult>(
            ["compose-standards", "--workspace", workspacePath],
            String.Empty);
        await AssertWorkspaceCommandAsync<WorkspaceArchitectureEvaluationResult>(
            ["evaluate-architecture", "--workspace", workspacePath],
            String.Empty);
        await AssertWorkspaceCommandAsync<ComplianceEvaluationResult>(
            ["validate-regulations", "--workspace", workspacePath],
            String.Empty);
        await AssertWorkspaceCommandAsync<ProjectSelectionProfile>(
            ["infer-project-selection", "--workspace", workspacePath],
            String.Empty);
        await AssertWorkspaceCommandAsync<ReportGenerationResult>(
            ["generate-reports", "--workspace", workspacePath],
            String.Empty);
        await AssertWorkspaceCommandAsync<AiInstructionGenerationRequest>(
            ["build-ai-instruction-request", "--workspace", workspacePath],
            String.Empty);

        await AssertWorkspaceCommandAsync<ProjectGenerationResult>(
            ["generate-project"],
            ContractJson.Serialize(projectSelection));
        await AssertWorkspaceCommandAsync<AiInstructionGenerationResult>(
            ["generate-ai-instructions"],
            ContractJson.Serialize(aiInstructionRequest));
    }

    [Fact]
    public async Task Cli_returns_non_zero_and_error_text_for_invalid_usage()
    {
        var output = new StringWriter();
        var error = new StringWriter();
        var app = ArchitectureStudioCliApp.CreateDefault(output, error, new StringReader(string.Empty));

        var exitCode = await app.InvokeAsync(["analyze-repository"]);

        Assert.NotEqual(0, exitCode);
        Assert.Contains("--workspace", error.ToString());
    }

    [Fact]
    public async Task Cli_standard_input_commands_accept_utf8_bom_payloads()
    {
        var workspacePath = Path.Combine(AppContext.BaseDirectory, "Fixtures", "SampleWorkspaces", "fintech-platform");
        var projectSelection = StudioWorkspaceOrchestrator.CreateDefault().InferProjectSelection(workspacePath);

        var output = new StringWriter();
        var error = new StringWriter();
        var app = ArchitectureStudioCliApp.CreateDefault(
            output,
            error,
            new StringReader("\uFEFF" + ContractJson.Serialize(projectSelection)));

        var exitCode = await app.InvokeAsync(["generate-project"]);

        Assert.Equal(0, exitCode);
        Assert.Empty(error.ToString());
        Assert.NotNull(ContractJson.Deserialize<ProjectGenerationResult>(output.ToString()));
    }

    [Fact]
    public async Task Packaged_cli_host_supports_workspace_and_standard_input_automation_flows()
    {
        var workspacePath = Path.Combine(AppContext.BaseDirectory, "Fixtures", "SampleWorkspaces", "fintech-platform");
        var orchestrator = StudioWorkspaceOrchestrator.CreateDefault();
        var projectSelection = orchestrator.InferProjectSelection(workspacePath);

        var help = await InvokeCliProcessAsync(["--help"], string.Empty);
        Assert.Equal(0, help.ExitCode);
        Assert.Contains("generate-ai-instructions", help.StandardOutput);

        var workspaceRun = await InvokeCliProcessAsync(
            ["analyze-repository", "--workspace", workspacePath],
            string.Empty);
        Assert.Equal(0, workspaceRun.ExitCode);
        Assert.NotNull(ContractJson.Deserialize<RepositoryAnalysisResult>(workspaceRun.StandardOutput));

        var standardInputRun = await InvokeCliProcessAsync(
            ["generate-project"],
            ContractJson.Serialize(projectSelection));
        Assert.Equal(0, standardInputRun.ExitCode);
        Assert.NotNull(ContractJson.Deserialize<ProjectGenerationResult>(standardInputRun.StandardOutput));
    }

    private static async Task AssertWorkspaceCommandAsync<T>(string[] args, string standardInput)
    {
        var output = new StringWriter();
        var error = new StringWriter();
        var app = ArchitectureStudioCliApp.CreateDefault(output, error, new StringReader(standardInput));

        var exitCode = await app.InvokeAsync(args);

        Assert.Equal(0, exitCode);
        Assert.Empty(error.ToString());
        Assert.False(string.IsNullOrWhiteSpace(output.ToString()));

        var payload = ContractJson.Deserialize<T>(output.ToString());
        Assert.NotNull(payload);
    }

    private static async Task<(int ExitCode, string StandardOutput, string StandardError)> InvokeCliProcessAsync(
        string[] args,
        string standardInput)
    {
        var cliAssemblyPath = Path.Combine(AppContext.BaseDirectory, "ArchitectureStudio.Cli.dll");
        Assert.True(File.Exists(cliAssemblyPath), $"CLI assembly '{cliAssemblyPath}' was not found.");

        var startInfo = new ProcessStartInfo("dotnet")
        {
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true
        };

        startInfo.ArgumentList.Add(cliAssemblyPath);
        foreach (var argument in args)
        {
            startInfo.ArgumentList.Add(argument);
        }

        using var process = Process.Start(startInfo);
        Assert.NotNull(process);

        if (!string.IsNullOrEmpty(standardInput))
        {
            await process.StandardInput.WriteAsync(standardInput);
        }

        process.StandardInput.Close();

        var standardOutput = await process.StandardOutput.ReadToEndAsync();
        var standardError = await process.StandardError.ReadToEndAsync();

        await process.WaitForExitAsync();

        return (process.ExitCode, standardOutput, standardError);
    }
}
