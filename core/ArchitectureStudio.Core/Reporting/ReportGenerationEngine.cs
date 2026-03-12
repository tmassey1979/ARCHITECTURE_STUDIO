using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text;

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
                RelativePath: $"{exportRoot}/architecture-report.pdf",
                Format: ArtifactFormat.Pdf,
                Content: BuildPdfReport(request))
        }
        .OrderBy(static file => file.RelativePath, StringComparer.Ordinal)
        .ToArray();

        var reportArtifacts = files
            .Where(static file => file.RelativePath.EndsWith("architecture-report.md", StringComparison.OrdinalIgnoreCase)
                || file.RelativePath.EndsWith("architecture-report.pdf", StringComparison.OrdinalIgnoreCase)
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
            PdfFallbackUsed: false);
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

    private static string BuildPdfReport(ReportGenerationRequest request)
    {
        var lines = BuildPdfLines(request);
        return BuildPdfDocument(lines);
    }

    private static IReadOnlyList<string> BuildPdfLines(ReportGenerationRequest request)
    {
        var lines = new List<string>
        {
            $"{request.ProjectName} Architecture Report",
            string.Empty,
            "Compliance Score Summary"
        };

        if (request.ComplianceSummaries.Count == 0)
        {
            lines.Add("- No compliance summaries were generated.");
        }
        else
        {
            lines.AddRange(request.ComplianceSummaries.Select(summary =>
                $"- {summary.RegulationTitle}: {summary.ScorePercentage}% ({summary.CoveredControls}/{summary.TotalControls} controls)"));
        }

        lines.Add(string.Empty);
        lines.Add("Findings");

        if (request.Findings.Count == 0)
        {
            lines.Add("- No findings were generated.");
        }
        else
        {
            foreach (var finding in request.Findings)
            {
                lines.Add($"- {finding.Severity}: {finding.Title}");
                lines.Add($"  Summary: {finding.Summary}");
                lines.Add($"  Remediation: {finding.Remediation.Title} - {finding.Remediation.Summary}");
                lines.Add(
                    finding.Evidence is { Count: > 0 }
                        ? $"  Evidence: {string.Join(", ", finding.Evidence)}"
                        : "  Evidence: No evidence paths recorded.");
            }
        }

        return lines;
    }

    private static string BuildPdfDocument(IReadOnlyList<string> lines)
    {
        const int pageHeight = 792;
        const int topMargin = 742;
        const int lineHeight = 16;
        const int maxLinesPerPage = 40;

        var pagedLines = lines
            .Chunk(maxLinesPerPage)
            .Select(static page => page.ToArray())
            .ToArray();

        if (pagedLines.Length == 0)
        {
            pagedLines = [["Architecture Studio Report"]];
        }

        var objects = new List<string>();
        var pageObjectNumbers = new List<int>();
        var nextObjectNumber = 4;

        foreach (var page in pagedLines)
        {
            var pageObjectNumber = nextObjectNumber++;
            var contentObjectNumber = nextObjectNumber++;
            var contentStream = BuildPdfContentStream(page, topMargin, lineHeight);

            objects.Add(
                $$"""
                {{pageObjectNumber}} 0 obj
                << /Type /Page /Parent 2 0 R /MediaBox [0 0 612 {{pageHeight}}] /Resources << /Font << /F1 3 0 R >> >> /Contents {{contentObjectNumber}} 0 R >>
                endobj
                """
            );
            objects.Add(
                $$"""
                {{contentObjectNumber}} 0 obj
                << /Length {{contentStream.Length}} >>
                stream
                {{contentStream}}
                endstream
                endobj
                """
            );

            pageObjectNumbers.Add(pageObjectNumber);
        }

        var pagesObject =
            $$"""
            2 0 obj
            << /Type /Pages /Count {{pageObjectNumbers.Count}} /Kids [{{string.Join(" ", pageObjectNumbers.Select(static number => $"{number} 0 R"))}}] >>
            endobj
            """;

        objects.Insert(0, pagesObject);
        objects.Insert(0, """
            1 0 obj
            << /Type /Catalog /Pages 2 0 R >>
            endobj
            """);
        objects.Insert(2, """
            3 0 obj
            << /Type /Font /Subtype /Type1 /BaseFont /Helvetica >>
            endobj
            """);

        var builder = new StringBuilder();
        builder.Append("%PDF-1.4\n");

        var xrefOffsets = new List<int> { 0 };
        foreach (var pdfObject in objects)
        {
            xrefOffsets.Add(builder.Length);
            builder.Append(pdfObject);
            builder.Append('\n');
        }

        var xrefStart = builder.Length;
        builder.Append("xref\n");
        builder.Append($"0 {xrefOffsets.Count}\n");
        builder.Append("0000000000 65535 f \n");

        foreach (var offset in xrefOffsets.Skip(1))
        {
            builder.Append($"{offset:D10} 00000 n \n");
        }

        builder.Append("trailer\n");
        builder.Append($"<< /Size {xrefOffsets.Count} /Root 1 0 R >>\n");
        builder.Append("startxref\n");
        builder.Append(xrefStart);
        builder.Append("\n%%EOF");

        return builder.ToString();
    }

    private static string BuildPdfContentStream(IReadOnlyList<string> lines, int topMargin, int lineHeight)
    {
        var builder = new StringBuilder();
        builder.Append("BT\n");
        builder.Append("/F1 12 Tf\n");
        builder.Append($"{lineHeight} TL\n");
        builder.Append($"50 {topMargin} Td\n");

        var firstLine = true;
        foreach (var line in lines)
        {
            if (!firstLine)
            {
                builder.Append("T*\n");
            }

            builder.Append('(');
            builder.Append(EscapePdfText(line));
            builder.Append(") Tj\n");
            firstLine = false;
        }

        builder.Append("ET");
        return builder.ToString();
    }

    private static string EscapePdfText(string value)
    {
        var builder = new StringBuilder(value.Length);

        foreach (var character in value)
        {
            switch (character)
            {
                case '\\':
                    builder.Append("\\\\");
                    break;
                case '(':
                    builder.Append("\\(");
                    break;
                case ')':
                    builder.Append("\\)");
                    break;
                default:
                    builder.Append(character is >= ' ' and <= '~' ? character : '?');
                    break;
            }
        }

        return builder.ToString();
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
