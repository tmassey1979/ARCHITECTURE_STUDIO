using ArchitectureStudio.Core;

var orchestrator = StudioWorkspaceOrchestrator.CreateDefault();

try
{
    if (args.Length == 0)
    {
        throw new InvalidOperationException("A command name is required.");
    }

    var commandName = args[0];

    switch (commandName)
    {
        case "analyze-repository":
            WriteJson(orchestrator.AnalyzeRepository(GetWorkspacePath(args)));
            break;
        case "compose-standards":
            WriteJson(orchestrator.ComposeStandards(GetWorkspacePath(args)));
            break;
        case "evaluate-architecture":
            WriteJson(orchestrator.EvaluateArchitecture(GetWorkspacePath(args)));
            break;
        case "validate-regulations":
            WriteJson(orchestrator.ValidateRegulations(GetWorkspacePath(args)));
            break;
        case "infer-project-selection":
            WriteJson(orchestrator.InferProjectSelection(GetWorkspacePath(args)));
            break;
        case "generate-project":
            WriteJson(orchestrator.GenerateProject(ReadJsonFromStandardInput<ProjectSelectionProfile>()));
            break;
        case "generate-reports":
            WriteJson(orchestrator.GenerateReports(GetWorkspacePath(args)));
            break;
        case "build-ai-instruction-request":
            WriteJson(orchestrator.BuildAiInstructionRequest(GetWorkspacePath(args)));
            break;
        case "generate-ai-instructions":
            WriteJson(orchestrator.GenerateAiInstructions(ReadJsonFromStandardInput<AiInstructionGenerationRequest>()));
            break;
        default:
            throw new InvalidOperationException($"Unknown command '{commandName}'.");
    }

    return 0;
}
catch (Exception exception)
{
    Console.Error.WriteLine(exception.Message);
    return 1;
}

static string GetWorkspacePath(IReadOnlyList<string> args)
{
    for (var index = 1; index < args.Count - 1; index++)
    {
        if (string.Equals(args[index], "--workspace", StringComparison.OrdinalIgnoreCase))
        {
            var workspacePath = args[index + 1];
            if (!string.IsNullOrWhiteSpace(workspacePath))
            {
                return workspacePath;
            }
        }
    }

    throw new InvalidOperationException("The --workspace argument is required.");
}

static T ReadJsonFromStandardInput<T>()
{
    var payload = Console.In.ReadToEnd();
    if (string.IsNullOrWhiteSpace(payload))
    {
        throw new InvalidOperationException("JSON input is required on standard input.");
    }

    return ContractJson.Deserialize<T>(payload)
        ?? throw new InvalidOperationException($"The JSON request could not be deserialized as {typeof(T).Name}.");
}

static void WriteJson<T>(T value)
{
    Console.Out.WriteLine(ContractJson.Serialize(value));
}
