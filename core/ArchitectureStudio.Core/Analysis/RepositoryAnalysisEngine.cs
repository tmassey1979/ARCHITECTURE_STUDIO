using System.Text.RegularExpressions;

namespace ArchitectureStudio.Core;

public sealed class RepositoryAnalysisEngine
{
    private static readonly StringComparer PathComparer = StringComparer.OrdinalIgnoreCase;
    private static readonly HashSet<string> IgnoredDirectories =
    [
        ".git",
        "bin",
        "obj",
        "node_modules",
        "dist",
        "out"
    ];

    private static readonly IReadOnlyList<SensitivePatternDefinition> SensitivePatterns =
    [
        new(
            SensitiveDataCategory.Personal,
            "Personal data indicator",
            0.90,
            [
                new Regex("customeremail|email|firstname|lastname|ssn|dateofbirth", RegexOptions.IgnoreCase | RegexOptions.Compiled)
            ]),
        new(
            SensitiveDataCategory.Financial,
            "Financial data indicator",
            0.92,
            [
                new Regex("creditcard|cardnumber|iban|routingnumber|accountnumber", RegexOptions.IgnoreCase | RegexOptions.Compiled)
            ]),
        new(
            SensitiveDataCategory.Health,
            "Health data indicator",
            0.94,
            [
                new Regex("patient|diagnosis|medicalrecord|phi", RegexOptions.IgnoreCase | RegexOptions.Compiled)
            ]),
        new(
            SensitiveDataCategory.ChildData,
            "Child data indicator",
            0.93,
            [
                new Regex("child|minor|parentalconsent|coppa", RegexOptions.IgnoreCase | RegexOptions.Compiled)
            ])
    ];

    public RepositoryAnalysisResult Analyze(string workspacePath)
    {
        if (string.IsNullOrWhiteSpace(workspacePath))
        {
            throw new ArgumentException("Workspace path is required.", nameof(workspacePath));
        }

        if (!Directory.Exists(workspacePath))
        {
            throw new DirectoryNotFoundException($"Workspace path '{workspacePath}' was not found.");
        }

        var files = EnumerateRepositoryFiles(workspacePath)
            .OrderBy(static file => file.RelativePath, StringComparer.Ordinal)
            .ToArray();

        var signals = new Dictionary<string, SignalBuilder>(StringComparer.OrdinalIgnoreCase);
        var classifications = new Dictionary<SensitiveDataCategory, SensitiveClassificationBuilder>();

        DetectLanguages(files, signals);
        DetectFrameworks(files, signals);
        DetectInfrastructure(files, signals);
        DetectCiCd(files, signals);
        DetectArchitecturePatterns(workspacePath, signals);
        DetectSensitiveData(files, classifications);

        return new RepositoryAnalysisResult(
            Signals: signals.Values
                .Select(static builder => builder.Build())
                .OrderBy(static signal => signal.Category)
                .ThenBy(static signal => signal.Label, StringComparer.Ordinal)
                .ThenBy(static signal => signal.Id, StringComparer.Ordinal)
                .ToArray(),
            SensitiveData: classifications.Values
                .Select(static builder => builder.Build())
                .OrderBy(static classification => classification.Category)
                .ToArray());
    }

    private static IReadOnlyList<RepositoryFile> EnumerateRepositoryFiles(string workspacePath)
    {
        var files = new List<RepositoryFile>();
        EnumerateRepositoryFilesRecursive(workspacePath, workspacePath, files);
        return files;
    }

    private static void EnumerateRepositoryFilesRecursive(string workspacePath, string currentDirectory, List<RepositoryFile> files)
    {
        foreach (var directory in Directory.GetDirectories(currentDirectory))
        {
            var directoryName = Path.GetFileName(directory);
            if (IgnoredDirectories.Contains(directoryName))
            {
                continue;
            }

            EnumerateRepositoryFilesRecursive(workspacePath, directory, files);
        }

        foreach (var filePath in Directory.GetFiles(currentDirectory))
        {
            var relativePath = Path.GetRelativePath(workspacePath, filePath)
                .Replace(Path.DirectorySeparatorChar, '/');
            var fileName = Path.GetFileName(filePath);
            var extension = Path.GetExtension(filePath);
            string? contents = null;

            if (ShouldReadTextContents(fileName, extension))
            {
                contents = File.ReadAllText(filePath);
            }

            files.Add(new RepositoryFile(filePath, relativePath, fileName, extension, contents));
        }
    }

    private static bool ShouldReadTextContents(string fileName, string extension)
    {
        return fileName.Equals("Dockerfile", StringComparison.OrdinalIgnoreCase)
            || fileName.Equals("Jenkinsfile", StringComparison.OrdinalIgnoreCase)
            || extension.Equals(".csproj", StringComparison.OrdinalIgnoreCase)
            || extension.Equals(".json", StringComparison.OrdinalIgnoreCase)
            || extension.Equals(".xml", StringComparison.OrdinalIgnoreCase)
            || extension.Equals(".yml", StringComparison.OrdinalIgnoreCase)
            || extension.Equals(".yaml", StringComparison.OrdinalIgnoreCase)
            || extension.Equals(".cs", StringComparison.OrdinalIgnoreCase)
            || extension.Equals(".java", StringComparison.OrdinalIgnoreCase)
            || extension.Equals(".ts", StringComparison.OrdinalIgnoreCase)
            || extension.Equals(".tsx", StringComparison.OrdinalIgnoreCase)
            || extension.Equals(".js", StringComparison.OrdinalIgnoreCase)
            || extension.Equals(".jsx", StringComparison.OrdinalIgnoreCase)
            || extension.Equals(".md", StringComparison.OrdinalIgnoreCase)
            || extension.Equals(".gradle", StringComparison.OrdinalIgnoreCase);
    }

    private static void DetectLanguages(IEnumerable<RepositoryFile> files, IDictionary<string, SignalBuilder> signals)
    {
        var csharpFiles = files.Where(static file => file.Extension.Equals(".cs", StringComparison.OrdinalIgnoreCase) || file.Extension.Equals(".csproj", StringComparison.OrdinalIgnoreCase)).ToArray();
        var javaFiles = files.Where(static file => file.Extension.Equals(".java", StringComparison.OrdinalIgnoreCase) || file.FileName.Equals("pom.xml", StringComparison.OrdinalIgnoreCase)).ToArray();
        var typeScriptFiles = files.Where(static file => file.Extension.Equals(".ts", StringComparison.OrdinalIgnoreCase)
            || file.Extension.Equals(".tsx", StringComparison.OrdinalIgnoreCase)
            || (file.FileName.Equals("package.json", StringComparison.OrdinalIgnoreCase)
                && file.Contents is not null
                && (file.Contents.Contains("\"react\"", StringComparison.OrdinalIgnoreCase)
                    || file.Contents.Contains("@angular/core", StringComparison.OrdinalIgnoreCase)))).ToArray();

        AddSignal(signals, "csharp", "C#", RepositorySignalCategory.Language, 0.88, "Detected C# source or project files.", csharpFiles.Select(static file => file.RelativePath));
        AddSignal(signals, "java", "Java", RepositorySignalCategory.Language, 0.84, "Detected Java source or Maven project files.", javaFiles.Select(static file => file.RelativePath));
        AddSignal(signals, "typescript", "TypeScript", RepositorySignalCategory.Language, 0.85, "Detected TypeScript or JavaScript frontend workspace files.", typeScriptFiles.Select(static file => file.RelativePath));
    }

    private static void DetectFrameworks(IEnumerable<RepositoryFile> files, IDictionary<string, SignalBuilder> signals)
    {
        var aspNetFiles = files.Where(static file =>
                file.Extension.Equals(".csproj", StringComparison.OrdinalIgnoreCase)
                && file.Contents is not null
                && file.Contents.Contains("Microsoft.NET.Sdk.Web", StringComparison.OrdinalIgnoreCase))
            .ToArray();
        var springFiles = files.Where(static file =>
                file.Contents is not null
                && (file.Contents.Contains("spring-boot", StringComparison.OrdinalIgnoreCase)
                    || file.Contents.Contains("org.springframework", StringComparison.OrdinalIgnoreCase)))
            .ToArray();
        var reactFiles = files.Where(static file =>
                file.FileName.Equals("package.json", StringComparison.OrdinalIgnoreCase)
                && file.Contents is not null
                && file.Contents.Contains("\"react\"", StringComparison.OrdinalIgnoreCase))
            .ToArray();
        var angularFiles = files.Where(static file =>
                file.FileName.Equals("package.json", StringComparison.OrdinalIgnoreCase)
                && file.Contents is not null
                && file.Contents.Contains("@angular/core", StringComparison.OrdinalIgnoreCase))
            .ToArray();

        AddSignal(signals, "aspnet-core", "ASP.NET Core", RepositorySignalCategory.Framework, 0.98, "Detected Microsoft.NET.Sdk.Web project.", aspNetFiles.Select(static file => file.RelativePath));
        AddSignal(signals, "spring-boot", "Spring Boot", RepositorySignalCategory.Framework, 0.96, "Detected Spring Boot dependency.", springFiles.Select(static file => file.RelativePath));
        AddSignal(signals, "react", "React", RepositorySignalCategory.Framework, 0.95, "Detected React dependency.", reactFiles.Select(static file => file.RelativePath));
        AddSignal(signals, "angular", "Angular", RepositorySignalCategory.Framework, 0.95, "Detected Angular dependency.", angularFiles.Select(static file => file.RelativePath));
    }

    private static void DetectInfrastructure(IEnumerable<RepositoryFile> files, IDictionary<string, SignalBuilder> signals)
    {
        var dockerFiles = files.Where(static file =>
                file.FileName.Equals("Dockerfile", StringComparison.OrdinalIgnoreCase)
                || file.FileName.StartsWith("docker-compose", StringComparison.OrdinalIgnoreCase))
            .ToArray();
        var kubernetesFiles = files.Where(static file =>
                (file.Extension.Equals(".yml", StringComparison.OrdinalIgnoreCase) || file.Extension.Equals(".yaml", StringComparison.OrdinalIgnoreCase))
                && file.Contents is not null
                && file.Contents.Contains("apiVersion", StringComparison.OrdinalIgnoreCase)
                && file.Contents.Contains("kind", StringComparison.OrdinalIgnoreCase))
            .ToArray();

        AddSignal(signals, "docker", "Docker", RepositorySignalCategory.Infrastructure, 0.94, "Detected container build or compose files.", dockerFiles.Select(static file => file.RelativePath));
        AddSignal(signals, "kubernetes", "Kubernetes", RepositorySignalCategory.Infrastructure, 0.93, "Detected Kubernetes manifest files.", kubernetesFiles.Select(static file => file.RelativePath));
    }

    private static void DetectCiCd(IEnumerable<RepositoryFile> files, IDictionary<string, SignalBuilder> signals)
    {
        var githubActionFiles = files.Where(static file =>
                file.RelativePath.StartsWith(".github/workflows/", StringComparison.OrdinalIgnoreCase)
                && (file.Extension.Equals(".yml", StringComparison.OrdinalIgnoreCase) || file.Extension.Equals(".yaml", StringComparison.OrdinalIgnoreCase)))
            .ToArray();
        var jenkinsFiles = files.Where(static file => file.FileName.Equals("Jenkinsfile", StringComparison.OrdinalIgnoreCase)).ToArray();

        AddSignal(signals, "github-actions", "GitHub Actions", RepositorySignalCategory.CiCd, 0.99, "Detected GitHub Actions workflow file.", githubActionFiles.Select(static file => file.RelativePath));
        AddSignal(signals, "jenkins", "Jenkins", RepositorySignalCategory.CiCd, 0.99, "Detected Jenkins pipeline file.", jenkinsFiles.Select(static file => file.RelativePath));
    }

    private static void DetectArchitecturePatterns(string workspacePath, IDictionary<string, SignalBuilder> signals)
    {
        var directories = Directory.EnumerateDirectories(workspacePath, "*", SearchOption.AllDirectories)
            .Select(path => Path.GetRelativePath(workspacePath, path).Replace(Path.DirectorySeparatorChar, '/'))
            .ToArray();

        var hasDomain = directories.Any(static path => path.Contains("/Domain", StringComparison.OrdinalIgnoreCase) || path.StartsWith("Domain", StringComparison.OrdinalIgnoreCase));
        var hasApplication = directories.Any(static path => path.Contains("/Application", StringComparison.OrdinalIgnoreCase) || path.StartsWith("Application", StringComparison.OrdinalIgnoreCase));
        var hasInfrastructure = directories.Any(static path => path.Contains("/Infrastructure", StringComparison.OrdinalIgnoreCase) || path.StartsWith("Infrastructure", StringComparison.OrdinalIgnoreCase));

        if (hasDomain && hasApplication && hasInfrastructure)
        {
            AddSignal(
                signals,
                "clean-architecture",
                "Clean Architecture",
                RepositorySignalCategory.ArchitecturePattern,
                0.86,
                "Detected domain, application, and infrastructure layers in the workspace.",
                directories.Where(static path =>
                    path.Contains("/Domain", StringComparison.OrdinalIgnoreCase)
                    || path.Contains("/Application", StringComparison.OrdinalIgnoreCase)
                    || path.Contains("/Infrastructure", StringComparison.OrdinalIgnoreCase)
                    || path.StartsWith("Domain", StringComparison.OrdinalIgnoreCase)
                    || path.StartsWith("Application", StringComparison.OrdinalIgnoreCase)
                    || path.StartsWith("Infrastructure", StringComparison.OrdinalIgnoreCase)));
        }
    }

    private static void DetectSensitiveData(IEnumerable<RepositoryFile> files, IDictionary<SensitiveDataCategory, SensitiveClassificationBuilder> classifications)
    {
        foreach (var file in files)
        {
            if (string.IsNullOrWhiteSpace(file.Contents))
            {
                continue;
            }

            var normalizedContents = file.Contents.Replace("_", string.Empty, StringComparison.Ordinal).Replace("-", string.Empty, StringComparison.Ordinal);

            foreach (var pattern in SensitivePatterns)
            {
                foreach (var regex in pattern.Patterns)
                {
                    var match = regex.Match(normalizedContents);
                    if (!match.Success)
                    {
                        continue;
                    }

                    if (!classifications.TryGetValue(pattern.Category, out var builder))
                    {
                        builder = new SensitiveClassificationBuilder(pattern.Category, pattern.Confidence);
                        classifications.Add(pattern.Category, builder);
                    }

                    builder.AddEvidence($"{pattern.Label} matched '{match.Value}' in {file.RelativePath}.", file.RelativePath);
                }
            }
        }
    }

    private static void AddSignal(
        IDictionary<string, SignalBuilder> signals,
        string id,
        string label,
        RepositorySignalCategory category,
        double confidence,
        string evidence,
        IEnumerable<string> affectedPaths)
    {
        var sortedPaths = affectedPaths
            .Where(static path => !string.IsNullOrWhiteSpace(path))
            .Distinct(PathComparer)
            .OrderBy(static path => path, StringComparer.Ordinal)
            .ToArray();

        if (sortedPaths.Length == 0)
        {
            return;
        }

        if (!signals.TryGetValue(id, out var builder))
        {
            builder = new SignalBuilder(id, label, category, confidence);
            signals.Add(id, builder);
        }

        builder.AddEvidence(evidence, sortedPaths);
    }

    private sealed record RepositoryFile(
        string FullPath,
        string RelativePath,
        string FileName,
        string Extension,
        string? Contents);

    private sealed record SensitivePatternDefinition(
        SensitiveDataCategory Category,
        string Label,
        double Confidence,
        IReadOnlyList<Regex> Patterns);

    private sealed class SignalBuilder
    {
        private readonly HashSet<string> _evidence = new(StringComparer.Ordinal);
        private readonly HashSet<string> _affectedPaths = new(PathComparer);

        public SignalBuilder(string id, string label, RepositorySignalCategory category, double confidence)
        {
            Id = id;
            Label = label;
            Category = category;
            Confidence = confidence;
        }

        public string Id { get; }

        public string Label { get; }

        public RepositorySignalCategory Category { get; }

        public double Confidence { get; }

        public void AddEvidence(string evidence, IEnumerable<string> affectedPaths)
        {
            _evidence.Add(evidence);

            foreach (var affectedPath in affectedPaths)
            {
                _affectedPaths.Add(affectedPath);
            }
        }

        public RepositorySignal Build()
        {
            return new RepositorySignal(
                Id,
                Label,
                Category,
                Confidence,
                _evidence.OrderBy(static item => item, StringComparer.Ordinal).ToArray(),
                _affectedPaths.OrderBy(static item => item, StringComparer.Ordinal).ToArray());
        }
    }

    private sealed class SensitiveClassificationBuilder
    {
        private readonly HashSet<string> _evidence = new(StringComparer.Ordinal);
        private readonly HashSet<string> _affectedPaths = new(PathComparer);

        public SensitiveClassificationBuilder(SensitiveDataCategory category, double confidence)
        {
            Category = category;
            Confidence = confidence;
        }

        public SensitiveDataCategory Category { get; }

        public double Confidence { get; }

        public void AddEvidence(string evidence, string affectedPath)
        {
            _evidence.Add(evidence);
            _affectedPaths.Add(affectedPath);
        }

        public SensitiveDataClassification Build()
        {
            return new SensitiveDataClassification(
                Category,
                Confidence,
                _evidence.OrderBy(static item => item, StringComparer.Ordinal).ToArray(),
                _affectedPaths.OrderBy(static item => item, StringComparer.Ordinal).ToArray());
        }
    }
}
