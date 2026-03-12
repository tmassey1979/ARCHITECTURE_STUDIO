namespace ArchitectureStudio.Core;

public sealed class StudioWorkspaceOrchestrator
{
    private static readonly StringComparer Comparer = StringComparer.OrdinalIgnoreCase;
    private static readonly IReadOnlyList<string> FrontendCandidates = ["react", "angular", "vue", "blazor", "react-native", "wpf"];
    private static readonly IReadOnlyList<string> BackendCandidates = ["aspnet-core", "spring-boot", "django", "fastapi"];
    private static readonly IReadOnlyList<string> ArchitectureCandidates =
        ["clean-architecture", "hexagonal-architecture", "onion-architecture", "layered-architecture", "vertical-slice-architecture", "microservices", "event-driven-architecture"];
    private static readonly IReadOnlyList<string> CiCdCandidates = ["github-actions", "jenkins", "gitlab-ci", "azure-pipelines", "circleci"];
    private static readonly IReadOnlyList<string> InfrastructureCandidates = ["docker", "kubernetes", "terraform", "helm", "aws", "azure", "gcp"];
    private static readonly IReadOnlyList<string> ObservabilityCandidates = ["opentelemetry", "prometheus", "grafana"];
    private static readonly HashSet<string> IgnoredDirectories = [".git", "bin", "obj", "node_modules", "dist", "out"];

    private readonly AiInstructionGenerationEngine _aiInstructionGenerationEngine;
    private readonly ComplianceEngine _complianceEngine;
    private readonly ProjectGenerationEngine _projectGenerationEngine;
    private readonly RepositoryAnalysisEngine _repositoryAnalysisEngine;
    private readonly ReportGenerationEngine _reportGenerationEngine;
    private readonly StandardsCompositionEngine _standardsCompositionEngine;
    private readonly TechnologyGraphEngine _technologyGraphEngine;

    public StudioWorkspaceOrchestrator(
        RepositoryAnalysisEngine repositoryAnalysisEngine,
        StandardsCompositionEngine standardsCompositionEngine,
        TechnologyGraphEngine technologyGraphEngine,
        ComplianceEngine complianceEngine,
        ProjectGenerationEngine projectGenerationEngine,
        ReportGenerationEngine reportGenerationEngine,
        AiInstructionGenerationEngine aiInstructionGenerationEngine)
    {
        _repositoryAnalysisEngine = repositoryAnalysisEngine;
        _standardsCompositionEngine = standardsCompositionEngine;
        _technologyGraphEngine = technologyGraphEngine;
        _complianceEngine = complianceEngine;
        _projectGenerationEngine = projectGenerationEngine;
        _reportGenerationEngine = reportGenerationEngine;
        _aiInstructionGenerationEngine = aiInstructionGenerationEngine;
    }

    public static StudioWorkspaceOrchestrator CreateDefault()
    {
        var runtimeCatalogs = StudioRuntimeCatalogFactory.CreateDefault();

        return new StudioWorkspaceOrchestrator(
            new RepositoryAnalysisEngine(),
            new StandardsCompositionEngine(runtimeCatalogs.StandardsCatalog),
            new TechnologyGraphEngine(runtimeCatalogs.TechnologyGraphCatalog),
            new ComplianceEngine(runtimeCatalogs.ComplianceCatalog),
            new ProjectGenerationEngine(runtimeCatalogs.ProjectTemplateCatalog),
            new ReportGenerationEngine(),
            new AiInstructionGenerationEngine());
    }

    public RepositoryAnalysisResult AnalyzeRepository(string workspacePath)
    {
        return _repositoryAnalysisEngine.Analyze(workspacePath);
    }

    public ProjectSelectionProfile InferProjectSelection(string workspacePath)
    {
        var context = CreateWorkspaceContext(workspacePath);
        return context.Selection;
    }

    public ComposedStandardsResult ComposeStandards(string workspacePath)
    {
        var context = CreateWorkspaceContext(workspacePath);
        return ComposeStandards(context);
    }

    public WorkspaceArchitectureEvaluationResult EvaluateArchitecture(string workspacePath)
    {
        var context = CreateWorkspaceContext(workspacePath);
        var selectedNodeIds = context.Analysis.Signals
            .Select(static signal => signal.Id)
            .Concat([context.Selection.Frontend, context.Selection.Backend, context.Selection.ArchitecturePattern])
            .Concat(context.Selection.CiCd)
            .Concat(context.Selection.Infrastructure)
            .Where(static id => !string.IsNullOrWhiteSpace(id))
            .Distinct(Comparer)
            .ToArray();

        var technologyEvaluation = _technologyGraphEngine.Evaluate(new TechnologyStackSelection(selectedNodeIds));
        var findings = _technologyGraphEngine.ValidateArchitecture(BuildArchitectureValidationRequest(workspacePath));

        return new WorkspaceArchitectureEvaluationResult(technologyEvaluation, findings);
    }

    public ComplianceEvaluationResult ValidateRegulations(string workspacePath)
    {
        var context = CreateWorkspaceContext(workspacePath);
        return context.Compliance;
    }

    public ProjectGenerationResult GenerateProject(string workspacePath)
    {
        var context = CreateWorkspaceContext(workspacePath);
        return _projectGenerationEngine.Generate(context.Selection);
    }

    public ProjectGenerationResult GenerateProject(ProjectSelectionProfile selection)
    {
        return _projectGenerationEngine.Generate(selection);
    }

    public ReportGenerationResult GenerateReports(string workspacePath)
    {
        var context = CreateWorkspaceContext(workspacePath);
        return _reportGenerationEngine.Generate(
            new ReportGenerationRequest(
                ProjectName: context.ProjectName,
                ComplianceSummaries: context.Compliance.Summaries,
                Findings: context.Compliance.Findings));
    }

    public AiInstructionGenerationRequest BuildAiInstructionRequest(string workspacePath)
    {
        var context = CreateWorkspaceContext(workspacePath);
        var standards = ComposeStandards(context);

        return new AiInstructionGenerationRequest(
            ProjectName: context.ProjectName,
            TargetKind: AiInstructionTargetKind.AnalyzedRepository,
            ProjectSelection: context.Selection,
            Standards: standards.Standards.Select(static standard => standard.Definition).ToArray(),
            ComplianceSummaries: context.Compliance.Summaries,
            Findings: context.Compliance.Findings);
    }

    public AiInstructionGenerationResult GenerateAiInstructions(string workspacePath)
    {
        return _aiInstructionGenerationEngine.Generate(BuildAiInstructionRequest(workspacePath));
    }

    public AiInstructionGenerationResult GenerateAiInstructions(AiInstructionGenerationRequest request)
    {
        return _aiInstructionGenerationEngine.Generate(request);
    }

    private WorkspaceContext CreateWorkspaceContext(string workspacePath)
    {
        var analysis = AnalyzeRepository(workspacePath);
        var compliance = EvaluateCompliance(analysis);
        var selection = InferProjectSelection(analysis, compliance.Summaries.Select(static summary => summary.RegulationId).ToArray());
        var projectName = CreateProjectName(workspacePath);

        return new WorkspaceContext(projectName, analysis, compliance, selection);
    }

    private ComposedStandardsResult ComposeStandards(WorkspaceContext context)
    {
        return _standardsCompositionEngine.Compose(
            new StandardsCompositionRequest(
                ProjectSelection: new StandardsProjectSelection(
                    Frontend: context.Selection.Frontend,
                    Backend: context.Selection.Backend,
                    ArchitecturePattern: context.Selection.ArchitecturePattern,
                    CiCd: context.Selection.CiCd,
                    Infrastructure: context.Selection.Infrastructure,
                    AdditionalSelections: context.Selection.ComplianceTargets),
                RepositoryCharacteristics: BuildRepositoryCharacteristics(context.Analysis)));
    }

    private ComplianceEvaluationResult EvaluateCompliance(RepositoryAnalysisResult analysis)
    {
        return _complianceEngine.Evaluate(
            new ComplianceEvaluationRequest(
                SystemCharacteristics: BuildSystemCharacteristics(analysis),
                RepositoryAnalysis: analysis,
                ImplementedControlIds: BuildImplementedControls(analysis)));
    }

    private static ProjectSelectionProfile InferProjectSelection(
        RepositoryAnalysisResult analysis,
        IReadOnlyList<string> complianceTargets)
    {
        var signalIds = analysis.Signals
            .Select(static signal => signal.Id)
            .ToHashSet(Comparer);

        return new ProjectSelectionProfile(
            Frontend: ResolvePrimarySelection(signalIds, FrontendCandidates, "react"),
            Backend: ResolvePrimarySelection(signalIds, BackendCandidates, "aspnet-core"),
            ArchitecturePattern: ResolvePrimarySelection(signalIds, ArchitectureCandidates, "clean-architecture"),
            CiCd: ResolveMultipleSelections(signalIds, CiCdCandidates, "github-actions"),
            Infrastructure: ResolveMultipleSelections(signalIds, InfrastructureCandidates, "docker"),
            ComplianceTargets: complianceTargets
                .Where(static value => !string.IsNullOrWhiteSpace(value))
                .Distinct(Comparer)
                .OrderBy(static value => value, StringComparer.Ordinal)
                .ToArray());
    }

    private static RepositoryCharacteristics BuildRepositoryCharacteristics(RepositoryAnalysisResult analysis)
    {
        var signalIds = analysis.Signals
            .Select(static signal => signal.Id)
            .ToHashSet(Comparer);
        var categories = new HashSet<StandardCategory>();
        var tags = new HashSet<string>(Comparer);

        if (signalIds.Overlaps(FrontendCandidates))
        {
            categories.Add(StandardCategory.Frontend);
        }

        if (signalIds.Overlaps(BackendCandidates))
        {
            categories.Add(StandardCategory.Backend);
        }

        if (signalIds.Overlaps(ArchitectureCandidates))
        {
            categories.Add(StandardCategory.Architecture);
        }

        if (signalIds.Overlaps(CiCdCandidates) || signalIds.Overlaps(InfrastructureCandidates))
        {
            categories.Add(StandardCategory.DevOps);
            categories.Add(StandardCategory.Process);
        }

        if (signalIds.Overlaps(ObservabilityCandidates))
        {
            categories.Add(StandardCategory.Observability);
            tags.Add("monitoring");
        }

        if (analysis.SensitiveData.Count > 0)
        {
            categories.Add(StandardCategory.Security);
            tags.Add("security");
            tags.Add("compliance");
        }

        if (signalIds.Overlaps(CiCdCandidates))
        {
            categories.Add(StandardCategory.Testing);
            tags.Add("testing");
            tags.Add("automation");
        }

        return new RepositoryCharacteristics(
            DetectedTechnologies: analysis.Signals
                .Select(static signal => signal.Id)
                .Distinct(Comparer)
                .OrderBy(static value => value, StringComparer.Ordinal)
                .ToArray(),
            DetectedTags: tags.OrderBy(static value => value, StringComparer.Ordinal).ToArray(),
            DetectedCategories: categories.OrderBy(static value => value).ToArray());
    }

    private static IReadOnlyList<string> BuildSystemCharacteristics(RepositoryAnalysisResult analysis)
    {
        var values = new HashSet<string>(Comparer);

        if (analysis.SensitiveData.Any(static item => item.Category == SensitiveDataCategory.Financial))
        {
            values.Add("payments");
        }

        if (analysis.SensitiveData.Any(static item => item.Category == SensitiveDataCategory.Health))
        {
            values.Add("patient-portal");
        }

        if (analysis.SensitiveData.Any(static item => item.Category == SensitiveDataCategory.ChildData))
        {
            values.Add("kids-app");
        }

        return values.OrderBy(static value => value, StringComparer.Ordinal).ToArray();
    }

    private static IReadOnlyList<string> BuildImplementedControls(RepositoryAnalysisResult analysis)
    {
        var values = new HashSet<string>(Comparer);
        var signalIds = analysis.Signals.Select(static signal => signal.Id).ToHashSet(Comparer);

        if (signalIds.Overlaps(CiCdCandidates))
        {
            values.Add("security-testing");
        }

        if (signalIds.Overlaps(ObservabilityCandidates))
        {
            values.Add("monitoring-alerting");
        }

        if (signalIds.Contains("keycloak"))
        {
            values.Add("access-control");
        }

        return values.OrderBy(static value => value, StringComparer.Ordinal).ToArray();
    }

    private static ArchitectureValidationRequest BuildArchitectureValidationRequest(string workspacePath)
    {
        var domainToInfrastructureReferences = new List<string>();
        var uiBusinessLogicFiles = new List<string>();
        var controllerDatabaseAccesses = new List<string>();
        var authenticationConfigured = false;

        foreach (var filePath in EnumerateTextFiles(workspacePath))
        {
            var relativePath = Path.GetRelativePath(workspacePath, filePath).Replace(Path.DirectorySeparatorChar, '/');
            var contents = File.ReadAllText(filePath);

            if (relativePath.Contains("/Domain/", StringComparison.OrdinalIgnoreCase)
                && contents.Contains("Infrastructure", StringComparison.OrdinalIgnoreCase))
            {
                domainToInfrastructureReferences.Add(relativePath);
            }

            if ((relativePath.Contains("/clients/", StringComparison.OrdinalIgnoreCase)
                 || relativePath.Contains("/ui/", StringComparison.OrdinalIgnoreCase)
                 || relativePath.Contains("/webview/", StringComparison.OrdinalIgnoreCase))
                && ContainsAny(contents, "calculate", "businessRule", "discount", "tax", "total")
                && ContainsAny(contents, "if ", "switch ", "for "))
            {
                uiBusinessLogicFiles.Add(relativePath);
            }

            if (relativePath.EndsWith("Controller.cs", StringComparison.OrdinalIgnoreCase)
                && ContainsAny(contents, "DbContext", ".Orders", ".Payments", ".SaveChanges("))
            {
                controllerDatabaseAccesses.Add(relativePath);
            }

            if (ContainsAny(contents, "AddAuthentication", "UseAuthentication", "keycloak", "oauth", "openidconnect", "Auth0"))
            {
                authenticationConfigured = true;
            }
        }

        return new ArchitectureValidationRequest(
            DomainToInfrastructureReferences: domainToInfrastructureReferences
                .Distinct(Comparer)
                .OrderBy(static value => value, StringComparer.Ordinal)
                .ToArray(),
            UiBusinessLogicFiles: uiBusinessLogicFiles
                .Distinct(Comparer)
                .OrderBy(static value => value, StringComparer.Ordinal)
                .ToArray(),
            ControllerDatabaseAccesses: controllerDatabaseAccesses
                .Distinct(Comparer)
                .OrderBy(static value => value, StringComparer.Ordinal)
                .ToArray(),
            AuthenticationConfigured: authenticationConfigured);
    }

    private static IEnumerable<string> EnumerateTextFiles(string workspacePath)
    {
        foreach (var filePath in Directory.EnumerateFiles(workspacePath, "*.*", SearchOption.AllDirectories))
        {
            var relativeDirectory = Path.GetDirectoryName(Path.GetRelativePath(workspacePath, filePath)) ?? string.Empty;
            var segments = relativeDirectory.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

            if (segments.Any(IgnoredDirectories.Contains))
            {
                continue;
            }

            var extension = Path.GetExtension(filePath);
            if (extension.Equals(".cs", StringComparison.OrdinalIgnoreCase)
                || extension.Equals(".ts", StringComparison.OrdinalIgnoreCase)
                || extension.Equals(".tsx", StringComparison.OrdinalIgnoreCase)
                || extension.Equals(".js", StringComparison.OrdinalIgnoreCase)
                || extension.Equals(".jsx", StringComparison.OrdinalIgnoreCase))
            {
                yield return filePath;
            }
        }
    }

    private static string CreateProjectName(string workspacePath)
    {
        var folderName = Path.GetFileName(Path.GetFullPath(workspacePath).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
        var parts = folderName
            .Replace("-", " ", StringComparison.Ordinal)
            .Replace("_", " ", StringComparison.Ordinal)
            .Split(' ', StringSplitOptions.RemoveEmptyEntries);

        return string.Join(
            ' ',
            parts.Select(static part =>
                part.Length == 0
                    ? string.Empty
                    : char.ToUpperInvariant(part[0]) + part[1..].ToLowerInvariant()));
    }

    private static string ResolvePrimarySelection(IReadOnlySet<string> signalIds, IReadOnlyList<string> candidates, string fallback)
    {
        return candidates.FirstOrDefault(signalIds.Contains) ?? fallback;
    }

    private static IReadOnlyList<string> ResolveMultipleSelections(
        IReadOnlySet<string> signalIds,
        IReadOnlyList<string> candidates,
        string fallback)
    {
        var values = candidates.Where(signalIds.Contains).ToArray();
        return values.Length > 0 ? values : [fallback];
    }

    private static bool ContainsAny(string contents, params string[] values)
    {
        return values.Any(value => contents.Contains(value, StringComparison.OrdinalIgnoreCase));
    }

    private sealed record WorkspaceContext(
        string ProjectName,
        RepositoryAnalysisResult Analysis,
        ComplianceEvaluationResult Compliance,
        ProjectSelectionProfile Selection);
}
