namespace ArchitectureStudio.Core;

public sealed class AiInstructionGenerationEngine
{
    public AiInstructionGenerationResult Generate(AiInstructionGenerationRequest request)
    {
        var files = new[]
        {
            new AiInstructionGeneratedFile(
                RelativePath: "AGENTS.md",
                Title: "AGENTS.md",
                Kind: GeneratedArtifactKind.AiInstructions,
                Content: BuildAgentsMarkdown(request)),
            new AiInstructionGeneratedFile(
                RelativePath: "docs/ai-instructions.md",
                Title: "AI Instructions",
                Kind: GeneratedArtifactKind.AiInstructions,
                Content: BuildAiInstructionsMarkdown(request))
        }
        .OrderBy(static file => file.RelativePath, StringComparer.Ordinal)
        .ToArray();

        var generatedArtifacts = files
            .Select(file => new GeneratedArtifact(
                Id: CreateArtifactId(file.RelativePath),
                Title: file.Title,
                Kind: file.Kind,
                RelativePath: file.RelativePath))
            .OrderBy(static artifact => artifact.RelativePath, StringComparer.Ordinal)
            .ToArray();

        return new AiInstructionGenerationResult(
            GeneratedArtifacts: generatedArtifacts,
            Files: files);
    }

    private static string BuildAgentsMarkdown(AiInstructionGenerationRequest request)
    {
        var standards = request.Standards
            .OrderBy(static item => item.Category)
            .ThenBy(static item => item.Title, StringComparer.Ordinal)
            .Select(static standard => $"- {standard.Title}: {standard.Summary}")
            .ToArray();
        var complianceSummaries = request.ComplianceSummaries
            .OrderBy(static item => item.RegulationTitle, StringComparer.Ordinal)
            .ThenBy(static item => item.RegulationId, StringComparer.Ordinal)
            .Select(static summary => $"- {summary.RegulationTitle} {summary.ScorePercentage}% ({summary.CoveredControls}/{summary.TotalControls} controls)")
            .ToArray();
        var remediationItems = request.Findings
            .OrderBy(static item => item.Severity)
            .ThenBy(static item => item.Title, StringComparer.Ordinal)
            .Select(static finding => $"- {finding.Title}: {finding.Remediation.Title} - {finding.Remediation.Summary}")
            .ToArray();

        return $"""
# {request.ProjectName} Agent Guide

## Purpose

Target kind: {request.TargetKind}

Use this file to guide AI-assisted development for the selected architecture, standards, devops stack, and compliance targets.

## Architecture Rules

- Frontend: {request.ProjectSelection.Frontend}
- Backend: {request.ProjectSelection.Backend}
- Architecture pattern: {request.ProjectSelection.ArchitecturePattern}
- Keep boundaries explicit between frontend, backend, and infrastructure layers.
- Preserve modular architecture and keep generated files aligned to the selected pattern.

## Coding Standards

- Follow Clean Code.
- Follow SOLID principles.
- Keep the solution modular and deterministic.
{string.Join(Environment.NewLine, standards)}

## DevOps Rules

- CI/CD platforms: {JoinValues(request.ProjectSelection.CiCd)}
- Infrastructure targets: {JoinValues(request.ProjectSelection.Infrastructure)}
- Keep pipeline, infrastructure, and documentation artifacts aligned to the selected delivery stack.

## Compliance Requirements

- Compliance targets: {JoinValues(request.ProjectSelection.ComplianceTargets)}
{string.Join(Environment.NewLine, complianceSummaries)}
{string.Join(Environment.NewLine, remediationItems)}
""";
    }

    private static string BuildAiInstructionsMarkdown(AiInstructionGenerationRequest request)
    {
        var standards = request.Standards
            .OrderBy(static item => item.Category)
            .ThenBy(static item => item.Title, StringComparer.Ordinal)
            .Select(static standard => $"- {standard.Title}: {standard.Summary}")
            .ToArray();
        var findings = request.Findings
            .OrderBy(static item => item.Severity)
            .ThenBy(static item => item.Title, StringComparer.Ordinal)
            .Select(static finding => $"- {finding.Severity}: {finding.Title} -> {finding.Remediation.Title}")
            .ToArray();

        return $"""
# AI Instructions

## Context

- Project: {request.ProjectName}
- Target kind: {request.TargetKind}
- Frontend: {request.ProjectSelection.Frontend}
- Backend: {request.ProjectSelection.Backend}
- Architecture pattern: {request.ProjectSelection.ArchitecturePattern}

## Coding Agent Directives

- Respect the current architecture pattern and selected delivery stack.
- Reuse canonical standards and compliance outputs instead of inventing parallel rules.
- Prefer focused, deterministic changes with accompanying tests.

## Standards Context

{string.Join(Environment.NewLine, standards)}

## Compliance Context

{string.Join(Environment.NewLine, findings)}
""";
    }

    private static string JoinValues(IReadOnlyList<string> values)
    {
        return values.Count == 0 ? "None" : string.Join(", ", values);
    }

    private static string CreateArtifactId(string relativePath)
    {
        return relativePath
            .Replace('\\', '-')
            .Replace('/', '-')
            .Replace('.', '-')
            .Trim('-')
            .ToLowerInvariant();
    }
}
