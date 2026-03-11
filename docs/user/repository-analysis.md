# Repository Analysis

## What It Does

Architecture Studio can now analyze the current repository and detect evidence that matters for architecture and compliance work.

The first release of repository analysis looks for:

- languages in use
- frameworks and platforms
- architecture-pattern signals
- infrastructure and deployment tooling
- CI/CD tooling
- sensitive-data indicators

## Examples Detected

The current analyzer can identify examples such as:

- ASP.NET Core
- Spring Boot
- React
- Angular
- Docker
- Kubernetes
- GitHub Actions
- Jenkins

It can also flag repository content that suggests:

- personal data
- financial data
- health data
- child-data handling

## Why This Matters

This gives later Architecture Studio workflows a grounded starting point instead of guessing from a blank form. Repository analysis will feed:

- architecture reasoning
- compliance scoring
- report generation
- project and pipeline recommendations

## Current Command Behavior

The `Analyze Repository` command now targets the active VS Code workspace automatically. You do not need to manually type a path for the current workspace.

In this stage, the feature focuses on the analysis engine and the workspace-aware command boundary. Richer visual output in the dashboard and deeper report integration will arrive in later stories.

## Analysis Output

Each detection is designed for auditability. The structured result includes:

- category
- confidence
- evidence
- affected paths

That makes it easier to understand why the plugin reached a conclusion and what parts of the repository were involved.

## Documentation Notes

Screenshots are not included yet because this story delivers the engine and command boundary before the full analysis UX is exposed in the dashboard. Screenshots will be added once the interactive result views are implemented.
