param(
    [string]$Repo = "tmassey1979/ARCHITECTURE_STUDIO"
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

function New-FeatureBody {
    param(
        [string]$Story,
        [string]$AcceptanceCriteria,
        [string]$DevNotes
    )

    $lifecycleChecklist = @'
## Lifecycle Checklist
- [ ] Add `inprogress` label when implementation starts.
- [ ] Add start comment with scope and plan.
- [ ] Add or update a failing test before production code changes.
- [ ] Add progress comments during work.
- [ ] Validate with build/tests/smoke checks.
- [ ] Add changelog file under `changelog/`.
- [ ] Add completion comment with validation, commit hash, and time spent.
- [ ] Remove `inprogress`.
- [ ] Close issue.
'@

    return (@"
## Story
$Story

## Implementation Rules
- Build with TDD. Start by adding or updating a failing automated test.
- Prefer C#/.NET 10 for domain, analysis, compliance, graph, generator, and reporting logic.
- Keep TypeScript focused on the VS Code extension host, webview UI, and thin integration boundaries.

## Acceptance Criteria
$AcceptanceCriteria

## Dev Notes
$DevNotes

$lifecycleChecklist
"@).Trim()
}

function New-EpicBody {
    param(
        [string]$ChildStories
    )

    $lifecycleChecklist = @'
## Lifecycle Checklist
- [ ] Add `inprogress` label when implementation starts.
- [ ] Add start comment with scope and plan.
- [ ] Add or update a failing test before production code changes in each child story.
- [ ] Add progress comments during work.
- [ ] Validate child stories with build/tests/smoke checks.
- [ ] Add changelog file under `changelog/` when the epic closes.
- [ ] Add completion comment with validation, commit hash, and time spent.
- [ ] Remove `inprogress`.
- [ ] Close issue.
'@

    return (@"
## Story
As an engineering organization, I would like Architecture Studio delivered as a modular VS Code extension, so that teams can generate architectures, analyze repositories, validate compliance, and produce actionable project outputs from one workspace tool.

## In Scope
- VS Code extension commands and dashboard experience
- Standards, architecture, compliance, analysis, generator, graph, and reporting modules
- AI instruction generation and AGENTS.md generation
- Extensibility for external standards and compliance packs
- Build, test, and packaging workflows for local release readiness

## Out of Scope
- Future-extension items called out in codex/studio.md, including architecture drift detection, runtime architecture analysis, SBOM generation, and supply chain security analysis
- Hosted backend services, cloud persistence, or SaaS delivery concerns not specified in the build spec

## Child Stories
$ChildStories

## Success Metrics
- Every command listed in codex/studio.md is available from the extension command palette.
- Repository analysis produces typed findings for technologies, architecture, data sensitivity, and compliance gaps.
- The shipped technology graph includes curated dataset coverage aligned to codex/data.md, including broad technology categories and relationship types at the stated scale.
- The shipped compliance dataset covers the regulations and expanded control taxonomy described across codex/studio.md and codex/data.md.
- Generated outputs include a reusable template library, docs, AI instructions, and report artifacts aligned to the dataset scope in codex/data.md.
- Core engines and rule-heavy logic are implemented in C# wherever the VS Code runtime boundary allows it.
- The extension can be built, tested, and packaged locally.

## Dev Notes
- Keep the solution aligned to the directory and module boundaries defined in codex/studio.md.
- Keep the VS Code extension surface in TypeScript and Node.js.
- Interpret the language note from codex/studio.md as the VS Code extension-shell requirement, not as a ban on .NET components behind that shell.
- Prefer net10.0 C# projects for engines and reusable business logic where practical.
- Treat the child stories below as the canonical delivery order unless implementation findings require a dependency change.
- Treat codex/data.md as required seed-scope input for the graph, compliance, and template work rather than optional future content.
- Child stories should continue using the same `DragonEnvelopes` feature-story lifecycle.

$lifecycleChecklist
"@).Trim()
}

function Ensure-Label {
    param(
        [string]$Repo,
        [hashtable]$ExistingLabels,
        [string]$Name,
        [string]$Color,
        [string]$Description
    )

    if ($ExistingLabels.ContainsKey($Name)) {
        return
    }

    gh label create $Name --repo $Repo --color $Color --description $Description | Out-Null
    $ExistingLabels[$Name] = $true
}

function Get-IssueNumberFromUrl {
    param([string]$Url)

    $match = [regex]::Match($Url.Trim(), "/issues/(?<number>\d+)$")
    if (-not $match.Success) {
        throw "Unable to parse issue number from '$Url'."
    }

    return [int]$match.Groups["number"].Value
}

function Ensure-Issue {
    param(
        [string]$Repo,
        [hashtable]$ExistingIssuesByTitle,
        [hashtable]$Spec
    )

    if ($ExistingIssuesByTitle.ContainsKey($Spec.Title)) {
        $issue = $ExistingIssuesByTitle[$Spec.Title]
        gh issue edit $issue.number --repo $Repo --body $Spec.Body | Out-Null
        foreach ($label in $Spec.Labels) {
            gh issue edit $issue.number --repo $Repo --add-label $label | Out-Null
        }
        Write-Host "Updated issue #$($issue.number): $($issue.title)"
        return $issue
    }

    $arguments = @("issue", "create", "--repo", $Repo, "--title", $Spec.Title, "--body", $Spec.Body)
    foreach ($label in $Spec.Labels) {
        $arguments += @("--label", $label)
    }

    $url = gh @arguments
    $issue = [pscustomobject]@{
        number = Get-IssueNumberFromUrl -Url $url
        title = $Spec.Title
        url = $url.Trim()
    }

    $ExistingIssuesByTitle[$Spec.Title] = $issue
    Write-Host "Created issue #$($issue.number): $($issue.title)"
    return $issue
}

$existingLabels = @{}
foreach ($label in (gh label list --repo $Repo --limit 200 --json name | ConvertFrom-Json)) {
    $existingLabels[$label.name] = $true
}

Ensure-Label -Repo $Repo -ExistingLabels $existingLabels -Name "epic" -Color "1d76db" -Description "Cross-story delivery tracking."
Ensure-Label -Repo $Repo -ExistingLabels $existingLabels -Name "feature" -Color "0e8a16" -Description "New functionality."
Ensure-Label -Repo $Repo -ExistingLabels $existingLabels -Name "task" -Color "fbca04" -Description "Planned implementation work."

$existingIssuesByTitle = @{}
foreach ($issue in (gh issue list --repo $Repo --state all --limit 200 --json number,title,url | ConvertFrom-Json)) {
    $existingIssuesByTitle[$issue.title] = $issue
}

$childSpecs = @(
    @{
        Key = "bootstrap"
        Title = "Feature: Bootstrap the Architecture Studio VS Code extension workspace"
        Labels = @("feature", "task")
        Story = "As a contributor, I would like a runnable VS Code extension workspace scaffolded, so that the Architecture Studio feature set can be built and validated on a stable foundation."
        AcceptanceCriteria = @'
- [ ] The repository contains a baseline VS Code extension scaffold aligned to the `studio.md` directory model, including `src`, `core`, `ui`, `standards`, `compliance`, `templates`, and `reports`.
- [ ] Package scripts support local build, lint, test, and package workflows.
- [ ] The extension manifest contributes the extension and baseline activation events needed for command registration.
- [ ] TypeScript configuration and module boundaries allow the feature engines to evolve without circular imports.
- [ ] Developer setup and local debug steps are documented in the repository.
'@
        DevNotes = @'
- This is the foundation for every downstream story.
- Keep the folder names and top-level modules aligned to `studio.md`.
- Introduce only the minimum placeholder implementation needed to unblock command, UI, and engine stories.
- Establish the project conventions that later stories will reuse for tests, diagnostics, and packaging.
'@
    }
    @{
        Key = "shared-contracts"
        Title = "Feature: Define shared contracts for findings, graphs, regulations, and generators"
        Labels = @("feature", "task")
        Story = "As a maintainer, I would like shared domain contracts for the extension, so that standards, analysis, compliance, graph, and generator modules all speak the same language."
        AcceptanceCriteria = @'
- [ ] Shared types exist for standards, regulations, controls, graph nodes and edges, findings, remediations, reports, and generated artifacts.
- [ ] Severity and risk levels are defined consistently as `Critical`, `High`, `Medium`, and `Low`.
- [ ] User-selection contracts cover frontend, backend, architecture pattern, CI/CD, infrastructure, and compliance targets.
- [ ] The shared model includes serialization or validation helpers needed at extension, UI, and engine boundaries.
- [ ] Module ownership and dependency rules for these contracts are documented.
'@
        DevNotes = @'
- Build this immediately after bootstrap so downstream stories do not invent their own schemas.
- The contract set should support dashboard rendering, report emission, and generator output without lossy transforms.
- Keep the contracts framework-agnostic; consumers should add adapters rather than mutating the core model.
- Use these types as the source of truth for severity, compliance scores, findings, and graph edges.
'@
    }
    @{
        Key = "commands"
        Title = "Feature: Register Architecture Studio commands and activation flow"
        Labels = @("feature", "task")
        Story = "As a VS Code user, I would like the Architecture Studio commands registered in the command palette, so that I can access each capability directly from the extension."
        AcceptanceCriteria = @'
- [ ] The extension registers the required commands from `studio.md`: Open Dashboard, Compose Standards, Analyze Repository, Validate Regulations, Generate Architecture, Generate Project, Generate Reports, and Generate AI Instructions.
- [ ] Each command routes through a centralized handler or application service layer rather than embedding logic directly in activation code.
- [ ] Errors are surfaced through a consistent output or notification channel.
- [ ] Command activation remains lazy so the extension does not fully load every module on startup.
- [ ] Command identifiers and invocation paths are documented for downstream tests and automation.
'@
        DevNotes = @'
- Depends on bootstrap and should consume the shared contracts where appropriate.
- This story should provide stable entry points even if downstream features still return placeholder output initially.
- Add a dedicated output channel early so later engines have a standard place to log work.
- Keep the command boundary thin; feature-specific behavior belongs in dedicated services.
'@
    }
    @{
        Key = "dashboard"
        Title = "Feature: Build the Architecture Studio dashboard webview shell"
        Labels = @("feature", "task")
        Story = "As a VS Code user, I would like a dashboard webview for Architecture Studio, so that I can inspect architecture, standards, compliance, reports, and repository analysis from one UI surface."
        AcceptanceCriteria = @'
- [ ] The dashboard webview renders the required top-level sections: Architecture, Standards, Compliance, Reports, and Repository Analysis.
- [ ] The extension host and webview communicate over a typed message bridge for state updates and command actions.
- [ ] Compliance summary cards and analysis panels can render placeholder or live findings using the shared contracts.
- [ ] Opening, closing, and reopening the dashboard does not leak event handlers or stale state.
- [ ] Webview assets are packaged with the extension and function in local debug runs.
'@
        DevNotes = @'
- Depends on bootstrap, commands, and shared contracts.
- Favor a shell that can host incremental feature slices instead of waiting for every engine to be complete.
- Keep the UI model decoupled from engine internals; the webview should consume DTOs from the extension host.
- The compliance dashboard called out in `studio.md` can be delivered as a section within this shell.
'@
    }
    @{
        Key = "standards"
        Title = "Feature: Implement the standards library and standards composition engine"
        Labels = @("feature", "task")
        Story = "As an architect, I would like a modular standards library and composition engine, so that Architecture Studio can assemble engineering guidance tailored to a project context."
        AcceptanceCriteria = @'
- [ ] Standards content is organized into the categories listed in `studio.md`: principles, architecture, frontend, backend, devops, testing, security, observability, and process.
- [ ] Seed standards include the named examples such as Clean Code, SOLID, DRY, KISS, YAGNI, CQRS, Event Sourcing, and the listed frontend, backend, and devops technologies.
- [ ] The standards engine can compose and return a standards set based on user selections or detected repository characteristics.
- [ ] Standards output is consumable by the dashboard, project generator, report generator, and AI-instruction workflow.
- [ ] The implementation leaves clear extension points for external standards packages.
'@
        DevNotes = @'
- Depends on bootstrap and shared contracts.
- Model standards in a way that later plugin packages can contribute content without rewriting the engine.
- Preserve source metadata so reports and AI prompts can explain why a standard was selected.
- Compose standards deterministically; order and grouping should not change unexpectedly between runs.
'@
    }
    @{
        Key = "architecture-engine"
        Title = "Feature: Implement the technology graph and architecture reasoning engine"
        Labels = @("feature", "task")
        Story = "As an architect, I would like a technology graph and architecture reasoning engine, so that the extension can generate and validate compatible architecture choices."
        AcceptanceCriteria = @'
- [ ] The graph model supports the required node types: technologies, frameworks, architecture patterns, regulations, and controls.
- [ ] The graph supports the required edge types: `requires`, `conflicts`, `pairs_with`, and `recommended_with`.
- [ ] The shipped graph dataset includes broad curated coverage aligned to `codex/data.md`, including frontend frameworks, backend frameworks, cloud services, databases, CI/CD tools, messaging, observability, security tools, and architecture patterns.
- [ ] The initial graph content is authored at the scale described in `codex/data.md` and is stored in a modular data format that can be validated and expanded.
- [ ] The architecture engine includes the patterns named in `studio.md`: Clean Architecture, Hexagonal Architecture, Onion Architecture, Layered Architecture, Vertical Slice Architecture, Microservices, and Event Driven Architecture.
- [ ] The engine can evaluate compatibility and produce recommendations from a selected technology stack.
- [ ] Architecture validation emits findings for the violations called out in `studio.md`, including domain-to-infrastructure references, business logic in UI, direct database access from controllers, and missing authentication.
'@
        DevNotes = @'
- Depends on shared contracts and should align with the standards engine for pattern metadata.
- `codex/data.md` makes the curated graph content itself a deliverable, not just the reasoning engine around it.
- Use typed findings with severity and remediation hooks so the reporting and compliance stories can reuse the output.
- Keep graph storage implementation swappable; the first version does not need a heavyweight database if an in-memory model satisfies the contract.
- Separate compatibility rules from graph persistence so rule coverage can be tested directly.
'@
    }
    @{
        Key = "analysis"
        Title = "Feature: Implement repository analysis and sensitive data classification"
        Labels = @("feature", "task")
        Story = "As a developer, I would like Architecture Studio to analyze a repository and classify sensitive data, so that downstream architecture and compliance checks are grounded in detected evidence."
        AcceptanceCriteria = @'
- [ ] Repository analysis detects languages, frameworks, architecture patterns, infrastructure, and CI/CD signals from the workspace.
- [ ] The analyzer can identify the examples named in `studio.md`, including ASP.NET, Spring, React, Angular, Docker, Kubernetes, GitHub Actions, and Jenkins.
- [ ] Data classification identifies personal, financial, health, and child-data indicators from repository content or configuration.
- [ ] Analyzer output is structured so the compliance engine and reports can consume evidence, confidence, and affected files or components.
- [ ] The Analyze Repository command can target the current VS Code workspace without requiring manual path entry.
'@
        DevNotes = @'
- Depends on bootstrap, shared contracts, and the command surface.
- Start with deterministic repository heuristics; avoid hidden network dependencies or cloud services.
- Classifier output should distinguish detected evidence from inferred risk so later reports remain auditable.
- Keep the analyzer modular so additional detectors can be added without changing the command contract.
'@
    }
    @{
        Key = "compliance-engine"
        Title = "Feature: Implement the compliance engine with scoring and remediation"
        Labels = @("feature", "task")
        Story = "As a compliance reviewer, I would like Architecture Studio to determine applicable regulations and score control coverage, so that teams can see their compliance posture and remediation path."
        AcceptanceCriteria = @'
- [ ] The engine detects applicable regulations from system characteristics, detected technologies, and classified data types.
- [ ] Required controls are evaluated and a compliance score is produced for each applicable regulation.
- [ ] Missing controls and remediation suggestions are emitted as typed findings.
- [ ] Compliance summaries can be rendered in the dashboard in the style described by `studio.md` (for example, `HIPAA 72%`).
- [ ] The Validate Regulations command produces repeatable output for the same repository inputs.
'@
        DevNotes = @'
- Depends on shared contracts and repository analysis; regulation content will be delivered by the regulation-library story.
- Follow the four-step validator flow from `studio.md`: detect characteristics, determine regulations, evaluate controls, generate score.
- Keep scoring explainable so reports can show why a system scored the way it did.
- Severity and remediation output must align with the report-generation story.
'@
    }
    @{
        Key = "regulation-library"
        Title = "Feature: Author the regulation and control libraries"
        Labels = @("feature", "task")
        Story = "As a compliance maintainer, I would like a regulation and control library encoded as data, so that the compliance engine can evaluate supported frameworks consistently."
        AcceptanceCriteria = @'
- [ ] Regulation modules exist for GDPR, CCPA, COPPA, HIPAA, HITECH, SOX, PCI DSS, ISO 27001, NIST Cybersecurity Framework, SOC2, TCPA, and CAN-SPAM.
- [ ] Regulation modules follow a consistent schema that includes `id`, `category`, `jurisdiction`, `required_controls`, and `data_types`.
- [ ] The control library includes the technical controls named in `studio.md`, including encryption, audit logging, role-based access control, secrets management, network segmentation, data retention, and consent management.
- [ ] The compliance dataset also covers the expansion scope from `codex/data.md`, including PIPEDA and a broader control taxonomy at approximately the stated scale.
- [ ] Regulation and control data can be consumed directly by the compliance engine without custom per-framework adapters.
- [ ] Test fixtures cover at least one representative evaluation path for each regulation family.
'@
        DevNotes = @'
- Depends on shared contracts and should land alongside or immediately after the compliance engine contracts are stable.
- Treat this as content plus schema validation, not just a hard-coded list.
- `codex/data.md` raises the scope beyond the baseline regulation list, so completeness includes the expanded privacy-law and control coverage there.
- Preserve source comments or metadata where useful so future updates can be audited.
- The library should remain extensible for external compliance packs delivered by the plugin system.
'@
    }
    @{
        Key = "generator"
        Title = "Feature: Implement project, pipeline, and infrastructure generation"
        Labels = @("feature", "task")
        Story = "As a solution designer, I would like Architecture Studio to generate project scaffolds, pipelines, and infrastructure assets, so that selected architectures can be turned into working starting points."
        AcceptanceCriteria = @'
- [ ] The generator accepts the user selections called out in `studio.md`: frontend, backend, architecture pattern, CI/CD, infrastructure, and compliance targets.
- [ ] Generated output includes the required structure areas such as `src`, `services`, `frontend`, `infrastructure`, `docs`, `docker`, and `k8s`.
- [ ] Pipeline generation supports GitHub Actions, GitLab CI, Jenkins, Azure DevOps, and CircleCI.
- [ ] Templates are modular and reusable rather than embedded in a single monolithic generator flow.
- [ ] The template library includes the multi-template dataset scope from `codex/data.md`, including at least the listed architecture, compliance, and devops variations.
- [ ] Generated artifacts include architecture-oriented documentation stubs aligned to the chosen pattern.
'@
        DevNotes = @'
- Depends on shared contracts, standards content, architecture reasoning, and compliance targets.
- Keep template data separate from command handlers so plugin packs can contribute future templates.
- Treat the project templates dataset in `codex/data.md` as a required deliverable, not an example-only appendix.
- Generated output should be deterministic for the same selection set.
- This story covers project, CI/CD, and infrastructure generation together because the spec treats them as a connected selection flow.
'@
    }
    @{
        Key = "reports"
        Title = "Feature: Implement report and documentation generation"
        Labels = @("feature", "task")
        Story = "As an engineering lead, I would like Architecture Studio to generate reports and supporting documentation, so that analysis and compliance results can be shared outside the extension."
        AcceptanceCriteria = @'
- [ ] Report generation supports Markdown, JSON, and SARIF outputs for architecture and compliance findings.
- [ ] Documentation generation produces the files named in `studio.md`: `engineering-playbook.md`, `security-policy.md`, `incident-response.md`, and `architecture.md`.
- [ ] Report artifacts preserve severity, remediation, evidence, and score data in a stable schema.
- [ ] Report output is written under `reports/` or another documented export path with deterministic file naming.
- [ ] PDF output is either implemented or explicitly downgraded with a documented fallback that does not break report generation.
'@
        DevNotes = @'
- Depends on shared contracts plus the outputs of standards, architecture, analysis, and compliance stories.
- Treat documentation generation as a first-class feature, not a side effect of report export.
- Keep report writers modular so formats can be added or disabled independently.
- SARIF output should map severity and remediation information cleanly for code-scanning consumers.
'@
    }
    @{
        Key = "ai-instructions"
        Title = "Feature: Implement AI instruction and AGENTS.md generation"
        Labels = @("feature", "task")
        Story = "As an AI-assisted development team, I would like Architecture Studio to generate AI instructions and an `AGENTS.md` file, so that coding agents can follow the selected architecture, standards, devops rules, and compliance requirements."
        AcceptanceCriteria = @'
- [ ] The extension exposes the `Architecture Studio: Generate AI Instructions` command described in `studio.md`.
- [ ] AI instruction output includes architecture rules, coding standards, devops rules, and compliance requirements.
- [ ] The generator can emit an `AGENTS.md` file for a generated project or an analyzed repository.
- [ ] Instruction content is derived from the selected or detected project profile rather than a static template.
- [ ] The generated output is compatible with the standards and compliance content authored elsewhere in the extension.
'@
        DevNotes = @'
- Depends on standards, architecture, compliance, and generator outputs.
- Keep prompt assembly structured so future plugins can contribute additional instruction sections.
- Generated guidance should reference the same canonical rules used elsewhere in the extension to avoid drift.
- This story should not invent a second copy of compliance or standards logic; it should compose existing outputs.
'@
    }
    @{
        Key = "plugins"
        Title = "Feature: Implement the external standards package plugin system"
        Labels = @("feature", "task")
        Story = "As a platform owner, I would like Architecture Studio to load external standards packages, so that new architecture, compliance, and template packs can extend the extension without core rewrites."
        AcceptanceCriteria = @'
- [ ] The extension can discover and load external packages such as `aws-architecture-pack`, `kafka-event-driven-pack`, and `banking-compliance-pack`.
- [ ] A documented plugin contract defines versioning, contribution points, and validation rules for external packages.
- [ ] Invalid or missing packages fail gracefully without breaking core extension functionality.
- [ ] External packages can contribute at least standards, regulation and control data, templates, or graph content.
- [ ] Plugin load status is surfaced in a user-visible way such as logs or the dashboard.
'@
        DevNotes = @'
- Depends on stable contracts from the standards, compliance, template, and graph stories.
- Start with a constrained trust model; do not assume arbitrary package execution is safe.
- Contribution loading should be explicit and observable so debugging third-party packs is possible.
- Keep the plugin boundary narrow enough that the core extension still works without any external packages installed.
'@
    }
    @{
        Key = "quality"
        Title = "Feature: Add automated tests, packaging, and release readiness"
        Labels = @("feature", "task")
        Story = "As a maintainer, I would like automated validation and packaging for Architecture Studio, so that the extension can be shipped and changed with confidence."
        AcceptanceCriteria = @'
- [ ] Automated tests cover command routing, core engines, repository-analysis fixtures, compliance evaluation paths, and template-generation smoke checks.
- [ ] A CI workflow runs the required build, lint, test, and package steps.
- [ ] The extension can be packaged into a local `.vsix` artifact.
- [ ] A sample workspace or curated fixtures exist for end-to-end smoke validation of analysis, compliance, and reporting flows.
- [ ] Release-oriented documentation explains setup, debug workflow, packaging, and known limitations.
'@
        DevNotes = @'
- Depends on the bootstrap story first, then should expand as each engine lands.
- Favor fast local validation plus a smaller end-to-end smoke path rather than only heavy integration suites.
- Packaging and CI should validate the exact commands contributors are expected to run locally.
- This story closes the loop on operational readiness for the full master-build scope.
'@
    }
)

foreach ($spec in $childSpecs) {
    $spec["Body"] = New-FeatureBody -Story $spec.Story -AcceptanceCriteria $spec.AcceptanceCriteria -DevNotes $spec.DevNotes
}

$createdChildren = foreach ($spec in $childSpecs) {
    Ensure-Issue -Repo $Repo -ExistingIssuesByTitle $existingIssuesByTitle -Spec $spec
}

$childStoryList = ($createdChildren | ForEach-Object {
    "- [ ] #$($_.number) $($_.title)"
}) -join [Environment]::NewLine

$epicSpec = @{
    Title = "Epic: Deliver the Architecture Studio master build"
    Labels = @("epic")
    Body = New-EpicBody -ChildStories $childStoryList
}

if ($existingIssuesByTitle.ContainsKey($epicSpec.Title)) {
    $epicIssue = $existingIssuesByTitle[$epicSpec.Title]
    gh issue edit $epicIssue.number --repo $Repo --body $epicSpec.Body | Out-Null
    Write-Host "Updated epic #$($epicIssue.number): $($epicIssue.title)"
}
else {
    $epicIssue = Ensure-Issue -Repo $Repo -ExistingIssuesByTitle $existingIssuesByTitle -Spec $epicSpec
}

Write-Host ""
Write-Host "Epic:"
Write-Host $epicIssue.url
Write-Host ""
Write-Host "Child stories:"
foreach ($issue in $createdChildren) {
    Write-Host $issue.url
}
