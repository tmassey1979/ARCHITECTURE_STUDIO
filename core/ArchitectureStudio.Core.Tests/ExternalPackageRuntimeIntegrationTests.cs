namespace ArchitectureStudio.Core.Tests;

public sealed class ExternalPackageRuntimeIntegrationTests
{
    [Fact]
    public void Default_catalogs_include_external_package_contributions()
    {
        var standardsCatalog = StandardsCatalog.CreateDefault();
        Assert.Contains(standardsCatalog.Packages, package => package.Id == "aws-architecture-pack");
        Assert.Contains(standardsCatalog.Standards, standard => standard.Id == "aws-well-architected");

        var complianceCatalog = ComplianceCatalog.CreateDefault();
        Assert.Contains("segregation-of-duties", complianceCatalog.ControlsById.Keys);
        Assert.Contains(complianceCatalog.Regulations, regulation => regulation.Id == "operational-resilience");

        var templateCatalog = ProjectTemplateCatalog.CreateDefault();
        Assert.Contains(templateCatalog.Templates, template => template.Id == "infra-aws-reference-stack");
        Assert.Contains(templateCatalog.Templates, template => template.Id == "project-kafka-eventing");

        var graph = TechnologyGraphCatalog.CreateDefault().Graph;
        Assert.Contains(graph.Nodes, node => node.Id == "aws-lambda");
        Assert.Contains(graph.Nodes, node => node.Id == "schema-registry");
    }

    [Fact]
    public void Default_runtime_uses_external_package_contributions_across_engines()
    {
        var standardsEngine = new StandardsCompositionEngine(StandardsCatalog.CreateDefault());
        var standardsResult = standardsEngine.Compose(
            new StandardsCompositionRequest(
                ProjectSelection: new StandardsProjectSelection(
                    Frontend: "react",
                    Backend: "aspnet-core",
                    ArchitecturePattern: "clean-architecture",
                    CiCd: ["github-actions"],
                    Infrastructure: ["aws"],
                    AdditionalSelections: [])));

        var awsStandard = Assert.Single(standardsResult.Standards, standard => standard.Definition.Id == "aws-well-architected");
        Assert.Equal("aws-architecture-pack", awsStandard.Source.PackageId);

        var graphEngine = new TechnologyGraphEngine(TechnologyGraphCatalog.CreateDefault());
        var graphEvaluation = graphEngine.Evaluate(new TechnologyStackSelection(["aws-lambda", "kafka"]));
        Assert.Contains(graphEvaluation.Recommendations, recommendation => recommendation.NodeId == "cloudwatch");
        Assert.Contains(graphEvaluation.Recommendations, recommendation => recommendation.NodeId == "schema-registry");

        var complianceEngine = new ComplianceEngine(ComplianceCatalog.CreateDefault());
        var complianceResult = complianceEngine.Evaluate(
            new ComplianceEvaluationRequest(
                SystemCharacteristics: ["payments"],
                RepositoryAnalysis: new RepositoryAnalysisResult(
                    Signals:
                    [
                        new RepositorySignal(
                            Id: "kafka",
                            Label: "Kafka",
                            Category: RepositorySignalCategory.Infrastructure,
                            Confidence: 0.95,
                            Evidence: ["Detected Kafka topology."],
                            AffectedPaths: ["docs/eventing/kafka-topology.md"])
                    ],
                    SensitiveData:
                    [
                        new SensitiveDataClassification(
                            Category: SensitiveDataCategory.Financial,
                            Confidence: 0.94,
                            Evidence: ["Financial data indicator matched in ledger.json."],
                            AffectedPaths: ["data/ledger.json"])
                    ]),
                ImplementedControlIds: []));

        Assert.Contains(complianceResult.Summaries, summary => summary.RegulationId == "operational-resilience");
        Assert.Contains(complianceResult.Findings, finding => finding.Id == "missing-control-operational-resilience-segregation-of-duties");

        var orchestrator = StudioWorkspaceOrchestrator.CreateDefault();
        var projectResult = orchestrator.GenerateProject(
            new ProjectSelectionProfile(
                Frontend: "react",
                Backend: "aspnet-core",
                ArchitecturePattern: "event-driven",
                CiCd: ["github-actions"],
                Infrastructure: ["aws"],
                ComplianceTargets: []));

        Assert.Contains(projectResult.TemplateIds, templateId => templateId == "infra-aws-reference-stack");
        Assert.Contains(projectResult.TemplateIds, templateId => templateId == "project-kafka-eventing");
        Assert.Contains(projectResult.GeneratedArtifacts, artifact => artifact.RelativePath == "infra/aws/README.md");
        Assert.Contains(projectResult.GeneratedArtifacts, artifact => artifact.RelativePath == "docs/eventing/kafka-topology.md");
    }
}
