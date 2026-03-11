namespace ArchitectureStudio.Core;

public sealed record ContractValidationResult(IReadOnlyList<string> Errors)
{
    public bool IsValid => Errors.Count == 0;
}

public static class ContractValidation
{
    public static ContractValidationResult ValidateProjectSelection(ProjectSelectionProfile profile)
    {
        ArgumentNullException.ThrowIfNull(profile);

        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(profile.Frontend))
        {
            errors.Add("Frontend is required.");
        }

        if (string.IsNullOrWhiteSpace(profile.Backend))
        {
            errors.Add("Backend is required.");
        }

        if (string.IsNullOrWhiteSpace(profile.ArchitecturePattern))
        {
            errors.Add("ArchitecturePattern is required.");
        }

        return new ContractValidationResult(errors);
    }

    public static ContractValidationResult ValidatePayload(SharedContractPayload payload)
    {
        ArgumentNullException.ThrowIfNull(payload);

        var errors = new List<string>();
        errors.AddRange(ValidateProjectSelection(payload.ProjectSelection).Errors);
        errors.AddRange(GetDuplicateIdErrors(payload.Standards.Select(item => item.Id), "standard"));
        errors.AddRange(GetDuplicateIdErrors(payload.Regulations.Select(item => item.Id), "regulation"));
        errors.AddRange(GetDuplicateIdErrors(payload.Controls.Select(item => item.Id), "control"));
        errors.AddRange(GetDuplicateIdErrors(payload.GraphNodes.Select(item => item.Id), "graph node"));
        errors.AddRange(GetDuplicateIdErrors(payload.ComplianceSummaries.Select(item => item.RegulationId), "compliance summary"));
        errors.AddRange(GetDuplicateIdErrors(payload.Findings.Select(item => item.Id), "finding"));
        errors.AddRange(GetDuplicateIdErrors(payload.Reports.Select(item => item.Id), "report artifact"));
        errors.AddRange(GetDuplicateIdErrors(payload.GeneratedArtifacts.Select(item => item.Id), "generated artifact"));

        var graphNodeIds = payload.GraphNodes.Select(node => node.Id).ToHashSet(StringComparer.OrdinalIgnoreCase);
        foreach (var edge in payload.GraphEdges)
        {
            if (!graphNodeIds.Contains(edge.SourceId))
            {
                errors.Add($"Graph edge source '{edge.SourceId}' does not reference a known graph node.");
            }

            if (!graphNodeIds.Contains(edge.TargetId))
            {
                errors.Add($"Graph edge target '{edge.TargetId}' does not reference a known graph node.");
            }
        }

        return new ContractValidationResult(errors);
    }

    private static IEnumerable<string> GetDuplicateIdErrors(IEnumerable<string> ids, string itemName)
    {
        return ids
            .Where(id => !string.IsNullOrWhiteSpace(id))
            .GroupBy(id => id, StringComparer.OrdinalIgnoreCase)
            .Where(group => group.Count() > 1)
            .Select(group => $"Duplicate {itemName} id '{group.Key}' was found.");
    }
}
