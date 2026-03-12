using System.CommandLine;
using System.CommandLine.Parsing;
using System.CommandLine.Invocation;
using ArchitectureStudio.Core;

namespace ArchitectureStudio.Cli;

public sealed class ArchitectureStudioCliApp
{
    private readonly TextReader _standardInput;
    private readonly TextWriter _standardOutput;
    private readonly TextWriter _standardError;
    private readonly RootCommand _rootCommand;

    public ArchitectureStudioCliApp(
        StudioWorkspaceOrchestrator orchestrator,
        TextWriter? standardOutput = null,
        TextWriter? standardError = null,
        TextReader? standardInput = null)
    {
        ArgumentNullException.ThrowIfNull(orchestrator);

        _standardInput = standardInput ?? Console.In;
        _standardOutput = standardOutput ?? Console.Out;
        _standardError = standardError ?? Console.Error;
        _rootCommand = CreateRootCommand(orchestrator);
    }

    public static ArchitectureStudioCliApp CreateDefault(
        TextWriter? standardOutput = null,
        TextWriter? standardError = null,
        TextReader? standardInput = null)
    {
        return new ArchitectureStudioCliApp(
            StudioWorkspaceOrchestrator.CreateDefault(),
            standardOutput,
            standardError,
            standardInput);
    }

    public async Task<int> InvokeAsync(string[] args, CancellationToken cancellationToken = default)
    {
        try
        {
            var parseResult = _rootCommand.Parse(args);
            return await parseResult.InvokeAsync(
                new InvocationConfiguration
                {
                    Output = _standardOutput,
                    Error = _standardError
                },
                cancellationToken);
        }
        catch (Exception exception)
        {
            _standardError.WriteLine(exception.Message);
            return 1;
        }
    }

    private RootCommand CreateRootCommand(StudioWorkspaceOrchestrator orchestrator)
    {
        var rootCommand = new RootCommand("Automation-friendly command-line entry point for Architecture Studio core workflows.")
        {
            CreateWorkspaceCommand(
                "analyze-repository",
                "Analyze a workspace for technologies, patterns, and sensitive-data signals.",
                workspacePath => orchestrator.AnalyzeRepository(workspacePath)),
            CreateWorkspaceCommand(
                "compose-standards",
                "Compose standards for the target workspace.",
                workspacePath => orchestrator.ComposeStandards(workspacePath)),
            CreateWorkspaceCommand(
                "evaluate-architecture",
                "Evaluate the workspace architecture and technology graph recommendations.",
                workspacePath => orchestrator.EvaluateArchitecture(workspacePath)),
            CreateWorkspaceCommand(
                "validate-regulations",
                "Evaluate applicable regulations and compliance findings for the workspace.",
                workspacePath => orchestrator.ValidateRegulations(workspacePath)),
            CreateWorkspaceCommand(
                "infer-project-selection",
                "Infer the project selection profile for the workspace.",
                workspacePath => orchestrator.InferProjectSelection(workspacePath)),
            CreateWorkspaceCommand(
                "generate-reports",
                "Generate reports for the workspace using the current standards and compliance context.",
                workspacePath => orchestrator.GenerateReports(workspacePath)),
            CreateWorkspaceCommand(
                "build-ai-instruction-request",
                "Build the AI-instruction request payload for the workspace.",
                workspacePath => orchestrator.BuildAiInstructionRequest(workspacePath)),
            CreateStandardInputCommand<ProjectSelectionProfile>(
                "generate-project",
                "Generate project artifacts from a JSON project-selection payload provided on standard input.",
                request => orchestrator.GenerateProject(request)),
            CreateStandardInputCommand<AiInstructionGenerationRequest>(
                "generate-ai-instructions",
                "Generate AI instructions from a JSON request payload provided on standard input.",
                request => orchestrator.GenerateAiInstructions(request))
        };

        return rootCommand;
    }

    private Command CreateWorkspaceCommand<T>(string name, string description, Func<string, T> execute)
    {
        var workspaceOption = new Option<string>("--workspace")
        {
            Description = "Path to the workspace directory to analyze.",
            Required = true
        };

        var command = new Command(name, description);
        command.Options.Add(workspaceOption);
        command.SetAction(parseResult =>
        {
            var workspacePath = parseResult.GetValue(workspaceOption);
            if (string.IsNullOrWhiteSpace(workspacePath))
            {
                throw new InvalidOperationException("The --workspace argument is required.");
            }

            WriteJson(execute(workspacePath));
            return 0;
        });

        return command;
    }

    private Command CreateStandardInputCommand<TInput>(string name, string description, Func<TInput, object> execute)
    {
        var command = new Command(name, description);
        command.SetAction(_ =>
        {
            WriteJson(execute(ReadJsonFromStandardInput<TInput>()));
            return 0;
        });

        return command;
    }

    private T ReadJsonFromStandardInput<T>()
    {
        var payload = _standardInput.ReadToEnd();
        var normalizedPayload = payload.TrimStart('\uFEFF').Trim();

        if (string.IsNullOrWhiteSpace(normalizedPayload))
        {
            throw new InvalidOperationException("JSON input is required on standard input.");
        }

        return ContractJson.Deserialize<T>(normalizedPayload)
            ?? throw new InvalidOperationException($"The JSON request could not be deserialized as {typeof(T).Name}.");
    }

    private void WriteJson<T>(T value)
    {
        _standardOutput.WriteLine(ContractJson.Serialize(value));
    }
}
