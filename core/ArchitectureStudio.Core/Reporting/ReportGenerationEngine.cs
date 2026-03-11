using System.Text.Json;
using System.Text.Json.Serialization;

namespace ArchitectureStudio.Core;

public sealed class ReportGenerationEngine
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true
    };

    static ReportGenerationEngine()
    {
        SerializerOptions.Converters.Add(new JsonStringEnumConverter());
    }

    public ReportGenerationResult Generate(ReportGenerationRequest request)
    {
        var exportRoot = string.IsNullOrWhiteSpace(request.ExportRoot) ? "reports" : request.ExportRoot.Trim().Replace('\\', '/');

        var files = new[]
        {
            new GeneratedReportFile(
                RelativePath: $"{exportRoot}/architecture-report.md",
                Format: ArtifactFormat.Markdown,
                Content: BuildArchitectureMarkdown(request)),
            new GeneratedReportFile(
                RelativePath: $"{exportRoot}/compliance-report.json",
                Format: ArtifactFormat.Json,
                Content: BuildComplianceJson(request)),
            new GeneratedReportFile(
                RelativePath: $"{exportRoot}/findings.sarif",
                Format: ArtifactFormat.Sarif,
                Content: BuildSarif(request)),
            new GeneratedReportFile(
                RelativePath: $"{exportRoot}/engineering-playbook.md",
                Format: ArtifactFormat.Markdown,
                Content: BuildPlaybookMarkdown(request)),
            new GeneratedReportFile(
                RelativePath: $"{exportRoot}/security-policy.md",
                Format: ArtifactFormat.Markdown,
                Content: BuildSecurityPolicyMarkdown(request)),
            new GeneratedReportFile(
                RelativePath: $"{exportRoot}/incident-response.md",
                Format: ArtifactFormat.Markdown,
                Content: BuildIncidentResponseMarkdown(request)),
            new GeneratedReportFile(
                RelativePath: $"{exportRoot}/architecture.md",
                Format: ArtifactFormat.Markdown,
                Content: BuildArchitectureDocumentMarkdown(request)),
            new GeneratedReportFile(
                RelativePath: $"{exportRoot}/pdf-fallback.md",
                Format: ArtifactFormat.Markdown,
                Content: BuildPdfFallbackMarkdown())
        }
        .OrderBy(static file => file.RelativePath, StringComparer.Ordinal)
        .ToArray();

        var reportArtifacts = files
            .Where(static file => file.RelativePath.EndsWith("architecture-report.md", StringComparison.OrdinalIgnoreCase)
                || file.RelativePath.EndsWith("compliance-report.json", StringComparison.OrdinalIgnoreCase)
                || file.RelativePath.EndsWith("findings.sarif", StringComparison.OrdinalIgnoreCase))
            .Select(file => new ReportArtifact(
                Id: CreateArtifactId(file.RelativePath),
                Title: Path.GetFileNameWithoutExtension(file.RelativePath),
                Format: file.Format,
                RelativePath: file.RelativePath))
            .OrderBy(static artifact => artifact.RelativePath, StringComparer.Ordinal)
            .ToArray();

        return new ReportGenerationResult(
            ReportArtifacts: reportArtifacts,
            Files: files,
            PdfFallbackUsed: true);
    }

    private static string BuildArchitectureMarkdown(ReportGenerationRequest request)
    {
        return $"""
# {request.ProjectName} Architecture Report

## Compliance Score Summary

{string.Join(Environment.NewLine, request.ComplianceSummaries.Select(summary =>
$"- {summary.RegulationTitle}: {summary.ScorePercentage}% ({summary.CoveredControls}/{summary.TotalControls} controls)"))}

## Findings

{string.Join(Environment.NewLine, request.Findings.Select(finding =>
$"- {finding.Severity}: {finding.Title} - {finding.Summary}"))}
""";
    }

    private static string BuildComplianceJson(ReportGenerationRequest request)
    {
        var payload = new
        {
            projectName = request.ProjectName,
            complianceSummaries = request.ComplianceSummaries,
            findings = request.Findings
        };

        return JsonSerializer.Serialize(payload, SerializerOptions);
    }

    private static string BuildSarif(ReportGenerationRequest request)
    {
        var sarifPayload = new
        {
            version = "2.1.0",
            runs = new[]
            {
                new
                {
                    tool = new
                    {
                        driver = new
                        {
                            name = "Architecture Studio",
                            rules = request.Findings.Select(finding => new
                            {
                                id = finding.Id,
                                name = finding.Title,
                                shortDescription = new { text = finding.Summary },
                                help = new { text = $"{finding.Remediation.Title}: {finding.Remediation.Summary}" }
                            })
                        }
                    },
                    results = request.Findings.Select(finding => new
                    {
                        ruleId = finding.Id,
                        level = MapSarifLevel(finding.Severity),
                        message = new { text = finding.Summary },
                        properties = new
                        {
                            severity = finding.Severity.ToString(),
                            risk = finding.Risk.ToString(),
                            remediation = finding.Remediation.Summary,
                            evidence = finding.Evidence ?? Array.Empty<string>()
                        }
                    })
                }
            }
        };

        return JsonSerializer.Serialize(sarifPayload, SerializerOptions);
    }

    private static string BuildPlaybookMarkdown(ReportGenerationRequest request)
    {
        return $"""
# Engineering Playbook

Project: {request.ProjectName}

- Review generated reports under `reports/`
- Track remediation items from the compliance findings
- Re-run the report engine after major architecture or compliance changes
""";
    }

    private static string BuildSecurityPolicyMarkdown(ReportGenerationRequest request)
    {
        return $"""
# Security Policy

Project: {request.ProjectName}

## Current Findings

{string.Join(Environment.NewLine, request.Findings.Select(finding => $"- {finding.Title}: {finding.Remediation.Title}"))}
""";
    }

    private static string BuildIncidentResponseMarkdown(ReportGenerationRequest request)
    {
        return $"""
# Incident Response

Project: {request.ProjectName}

- Capture evidence from generated reports
- Escalate critical and high-severity findings
- Track remediation status for compliance gaps
""";
    }

    private static string BuildArchitectureDocumentMarkdown(ReportGenerationRequest request)
    {
        return $"""
# Architecture

Project: {request.ProjectName}

## Compliance Signals

{string.Join(Environment.NewLine, request.ComplianceSummaries.Select(summary => $"- {summary.RegulationTitle}: {summary.ScorePercentage}%"))}
""";
    }

    private static string BuildPdfFallbackMarkdown()
    {
        return """
# PDF Fallback

PDF report generation is not implemented yet.

Use the generated Markdown reports as the documented fallback export path.
""";
    }

    private static string MapSarifLevel(SeverityLevel severity)
    {
        return severity switch
        {
            SeverityLevel.Critical => "error",
            SeverityLevel.High => "warning",
            SeverityLevel.Medium => "warning",
            _ => "note"
        };
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
