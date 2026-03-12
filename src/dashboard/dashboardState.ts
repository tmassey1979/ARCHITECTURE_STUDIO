import type { RepositoryAnalysisResult } from "../analysis/repositoryAnalysis";
import type {
  ComplianceSummary,
  FindingDefinition,
  GeneratedArtifact,
  GraphEdgeDefinition,
  GraphNodeDefinition,
  ProjectSelectionProfile,
  ReportArtifact,
  SharedContractPayload,
  StandardDefinition
} from "../contracts/sharedContracts";
import type { ExternalPackageLoadStatus } from "../plugins/externalPackages";

export type DashboardSectionId =
  | "architecture"
  | "standards"
  | "compliance"
  | "reports"
  | "repository-analysis";

export type DashboardCardTone = "neutral" | "positive" | "warning" | "critical";

export type DashboardSummaryCard = {
  readonly title: string;
  readonly value: string;
  readonly detail: string;
  readonly tone: DashboardCardTone;
};

export type DashboardPanel = {
  readonly title: string;
  readonly items: readonly string[];
  readonly commandId?: string;
};

export type DashboardSection = {
  readonly id: DashboardSectionId;
  readonly title: string;
  readonly description: string;
  readonly cards: readonly DashboardSummaryCard[];
  readonly panels: readonly DashboardPanel[];
};

export type DashboardState = {
  readonly generatedAt: string;
  readonly title: string;
  readonly subtitle: string;
  readonly sections: readonly DashboardSection[];
};

export type DashboardStateOptions = {
  readonly externalPackageStatuses?: readonly ExternalPackageLoadStatus[];
  readonly hasWorkspace?: boolean;
  readonly repositoryAnalysis?: RepositoryAnalysisResult;
  readonly subtitle?: string;
  readonly workspaceLabel?: string;
};

export type DashboardHostMessage = {
  readonly type: "dashboard.stateChanged";
  readonly state: DashboardState;
};

export type DashboardWebviewMessage =
  | {
      readonly type: "dashboard.ready";
    }
  | {
      readonly type: "dashboard.runCommand";
      readonly commandId: string;
    };

const emptyRepositoryAnalysis: RepositoryAnalysisResult = {
  signals: [],
  sensitiveData: []
};

const emptyProjectSelection: ProjectSelectionProfile = {
  frontend: "Not analyzed",
  backend: "Not analyzed",
  architecturePattern: "Not analyzed",
  ciCd: [],
  infrastructure: [],
  complianceTargets: []
};

const emptyPayload: SharedContractPayload = {
  standards: [],
  regulations: [],
  controls: [],
  graphNodes: [],
  graphEdges: [],
  findings: [],
  complianceSummaries: [],
  reports: [],
  generatedArtifacts: [],
  projectSelection: emptyProjectSelection
};

const samplePayload: SharedContractPayload = {
  standards: [
    {
      id: "std-arch-layering",
      title: "Service Layering",
      category: "Architecture",
      summary: "Keep webview, orchestration, and domain boundaries explicit.",
      tags: ["modularity", "boundaries", "maintainability"]
    },
    {
      id: "std-testing-tdd",
      title: "Test Driven Delivery",
      category: "Testing",
      summary: "Drive stories through failing tests first and keep regression coverage in place.",
      tags: ["tdd", "quality", "automation"]
    }
  ],
  regulations: [
    {
      id: "reg-soc2-cc7",
      category: "Security",
      jurisdiction: "Global",
      requiredControls: ["ctrl-secrets", "ctrl-audit"],
      dataTypes: ["SourceCode", "Configuration"]
    }
  ],
  controls: [
    {
      id: "ctrl-secrets",
      title: "Secret Detection",
      summary: "Detect credentials and rotate compromised values."
    },
    {
      id: "ctrl-audit",
      title: "Audit Logging",
      summary: "Capture change history and security-relevant events."
    }
  ],
  graphNodes: [
    {
      id: "node-vscode",
      label: "VS Code Extension Host",
      category: "Technology"
    },
    {
      id: "node-webview",
      label: "Dashboard Webview",
      category: "ArchitecturePattern"
    },
    {
      id: "node-dotnet",
      label: ".NET Analysis Engine",
      category: "Framework"
    }
  ],
  graphEdges: [
    {
      sourceId: "node-vscode",
      targetId: "node-webview",
      relationship: "Requires"
    },
    {
      sourceId: "node-webview",
      targetId: "node-dotnet",
      relationship: "PairsWith"
    }
  ],
  findings: [
    {
      id: "finding-secret-scan",
      title: "Secrets scanning sample finding",
      summary: "Repository analysis should validate that committed configuration is scrubbed of credentials.",
      severity: "High",
      risk: "Medium",
      remediation: {
        title: "Move secrets into secure storage",
        summary: "Replace inline credentials with a vault-backed configuration source."
      },
      evidence: ["src/appsettings.Development.json", "pipelines/build.yml"]
    },
    {
      id: "finding-control-gap",
      title: "Control mapping sample finding",
      summary: "Compliance summaries should show when required controls are only partially covered.",
      severity: "Medium",
      risk: "Medium",
      remediation: {
        title: "Complete control mapping",
        summary: "Link the regulation requirement to enforceable control definitions."
      },
      evidence: ["controls/coverage-map.json"]
    }
  ],
  complianceSummaries: [
    {
      regulationId: "soc2",
      regulationTitle: "SOC 2",
      scorePercentage: 68,
      coveredControls: 2,
      totalControls: 3
    },
    {
      regulationId: "security-baseline",
      regulationTitle: "Security Baseline",
      scorePercentage: 75,
      coveredControls: 3,
      totalControls: 4
    }
  ],
  reports: [
    {
      id: "report-architecture-overview",
      title: "Architecture Overview",
      format: "Markdown",
      relativePath: "reports/architecture-overview.md"
    },
    {
      id: "report-compliance-summary",
      title: "Compliance Summary",
      format: "Markdown",
      relativePath: "reports/compliance-summary.md"
    }
  ],
  generatedArtifacts: [
    {
      id: "artifact-agents",
      title: "AGENTS.md",
      kind: "AiInstructions",
      relativePath: "AGENTS.md"
    },
    {
      id: "artifact-template",
      title: "Project Scaffold",
      kind: "ProjectScaffold",
      relativePath: "templates/projects/README.md"
    }
  ],
  projectSelection: {
    frontend: "VS Code Webview",
    backend: ".NET Architecture Engines",
    architecturePattern: "Modular Extension Shell",
    ciCd: ["GitHub Actions"],
    infrastructure: ["GitHub Pages", "Visual Studio Marketplace"],
    complianceTargets: ["SOC 2", "Security Baseline"]
  }
};

function mergeProjectSelection(
  base: ProjectSelectionProfile,
  selection?: Partial<ProjectSelectionProfile>
): ProjectSelectionProfile {
  if (!selection) {
    return base;
  }

  return {
    ...base,
    ...selection,
    ciCd: selection.ciCd ?? base.ciCd,
    infrastructure: selection.infrastructure ?? base.infrastructure,
    complianceTargets: selection.complianceTargets ?? base.complianceTargets
  };
}

function mergeSharedContractPayload(
  base: SharedContractPayload,
  overrides: Partial<SharedContractPayload>
): SharedContractPayload {
  return {
    ...base,
    ...overrides,
    standards: overrides.standards ?? base.standards,
    regulations: overrides.regulations ?? base.regulations,
    controls: overrides.controls ?? base.controls,
    graphNodes: overrides.graphNodes ?? base.graphNodes,
    graphEdges: overrides.graphEdges ?? base.graphEdges,
    complianceSummaries: overrides.complianceSummaries ?? base.complianceSummaries,
    findings: overrides.findings ?? base.findings,
    reports: overrides.reports ?? base.reports,
    generatedArtifacts: overrides.generatedArtifacts ?? base.generatedArtifacts,
    projectSelection: mergeProjectSelection(base.projectSelection, overrides.projectSelection)
  };
}

function countEvidence(findings: readonly FindingDefinition[]): number {
  return findings.reduce((total, finding) => total + (finding.evidence?.length ?? 0), 0);
}

function joinValues(values: readonly string[], fallback: string): string {
  return values.length > 0 ? values.join(", ") : fallback;
}

function withFallbackItems(items: readonly string[], fallback: string): readonly string[] {
  return items.length > 0 ? items : [fallback];
}

function confidencePercentage(value: number): string {
  return `${Math.round(value * 100)}%`;
}

function createWorkspaceFallbackMessage(hasWorkspace: boolean, emptyWorkspaceMessage: string): string {
  return hasWorkspace ? emptyWorkspaceMessage : "Open a workspace folder to load live Architecture Studio data.";
}

function createArchitectureSection(
  graphNodes: readonly GraphNodeDefinition[],
  graphEdges: readonly GraphEdgeDefinition[],
  selection: ProjectSelectionProfile,
  hasWorkspace: boolean
): DashboardSection {
  const deliveryStackItems = hasWorkspace
    ? [
        `Frontend: ${selection.frontend}`,
        `Backend: ${selection.backend}`,
        `Infrastructure: ${joinValues(selection.infrastructure, "None detected")}`
      ]
    : [createWorkspaceFallbackMessage(false, "No delivery stack data is available for the current workspace.")];

  return {
    id: "architecture",
    title: "Architecture",
    description: "Review the active architecture model, graph relationships, and current build target stack.",
    cards: [
      {
        title: "Graph Nodes",
        value: String(graphNodes.length),
        detail: hasWorkspace ? "Technologies and patterns detected for the active workspace." : "Open a workspace to load graph data.",
        tone: graphNodes.length > 0 ? "positive" : "warning"
      },
      {
        title: "Graph Edges",
        value: String(graphEdges.length),
        detail: hasWorkspace ? "Compatibility, requirement, and recommendation links." : "Workspace-driven graph links appear here.",
        tone: graphEdges.length > 0 ? "positive" : "warning"
      },
      {
        title: "Target Pattern",
        value: selection.architecturePattern,
        detail: hasWorkspace
          ? `${selection.frontend} with ${selection.backend}.`
          : "Open a workspace folder to infer the target architecture pattern.",
        tone: hasWorkspace ? "neutral" : "warning"
      }
    ],
    panels: [
      {
        title: "Primary Components",
        items: withFallbackItems(
          graphNodes.map((node) => `${node.label} (${node.category})`),
          createWorkspaceFallbackMessage(hasWorkspace, "No architecture components were inferred for the current workspace.")
        ),
        commandId: "architectureStudio.generateArchitecture"
      },
      {
        title: "Delivery Stack",
        items: deliveryStackItems,
        commandId: "architectureStudio.generateProject"
      }
    ]
  };
}

function createStandardsSection(
  standards: readonly StandardDefinition[],
  selection: ProjectSelectionProfile,
  externalPackageStatuses: readonly ExternalPackageLoadStatus[],
  hasWorkspace: boolean
): DashboardSection {
  const externalPackCount = externalPackageStatuses.length;
  const externalPackItems =
    externalPackCount > 0
      ? externalPackageStatuses.map((status) => {
          const contributionKinds =
            status.contributionKinds.length > 0 ? ` [${status.contributionKinds.join(", ")}]` : "";
          return `${status.packageId} - ${status.status}: ${status.message}${contributionKinds}`;
        })
      : ["No external packages discovered yet."];

  return {
    id: "standards",
    title: "Standards",
    description: "Track the standards that inform architecture, delivery, and compliance decisions for the workspace.",
    cards: [
      {
        title: "Active Standards",
        value: String(standards.length),
        detail: hasWorkspace ? "Curated standards selected for the current workspace." : "Open a workspace to compose standards.",
        tone: standards.length > 0 ? "positive" : "warning"
      },
      {
        title: "Compliance Targets",
        value: String(selection.complianceTargets.length),
        detail: joinValues(
          selection.complianceTargets,
          hasWorkspace ? "No compliance targets inferred yet." : "Workspace inference will surface compliance targets here."
        ),
        tone: selection.complianceTargets.length > 0 ? "neutral" : "warning"
      },
      {
        title: "External Packs",
        value: String(externalPackCount),
        detail:
          externalPackCount > 0
            ? "Package load status is surfaced directly in the standards workspace."
            : "No external standards packages have been discovered yet.",
        tone: externalPackCount > 0 ? "neutral" : "warning"
      }
    ],
    panels: [
      {
        title: "Current Standard Set",
        items: withFallbackItems(
          standards.map((standard) => `${standard.title}: ${standard.summary}`),
          createWorkspaceFallbackMessage(hasWorkspace, "No standards were composed for the current workspace.")
        ),
        commandId: "architectureStudio.composeStandards"
      },
      {
        title: "External Package Status",
        items: hasWorkspace
          ? externalPackItems
          : [createWorkspaceFallbackMessage(false, "No external packages discovered yet.")],
        commandId: "architectureStudio.composeStandards"
      }
    ]
  };
}

function createComplianceSection(
  complianceSummaries: readonly ComplianceSummary[],
  findings: readonly FindingDefinition[],
  generatedArtifacts: readonly GeneratedArtifact[],
  hasWorkspace: boolean
): DashboardSection {
  const summaryCards =
    complianceSummaries.length > 0
      ? complianceSummaries.map((summary) => ({
          title: summary.regulationTitle,
          value: `${summary.scorePercentage}%`,
          detail: `${summary.coveredControls}/${summary.totalControls} controls covered.`,
          tone:
            summary.scorePercentage >= 85
              ? ("positive" as const)
              : summary.scorePercentage >= 70
                ? ("warning" as const)
                : ("critical" as const)
        }))
      : [
          {
            title: "Applicable Regulations",
            value: "0",
            detail: hasWorkspace ? "No regulations were inferred for the current workspace." : "Open a workspace to evaluate regulations.",
            tone: "warning" as const
          },
          {
            title: "Findings",
            value: String(findings.length),
            detail: hasWorkspace ? "Compliance and architecture findings currently in scope." : "Workspace findings will appear here.",
            tone: findings.length > 0 ? ("warning" as const) : ("neutral" as const)
          }
        ];

  return {
    id: "compliance",
    title: "Compliance",
    description: "Summarize findings, control coverage, and remediation work from the compliance engine.",
    cards: [
      ...summaryCards,
      {
        title: "Generated Guidance",
        value: String(generatedArtifacts.length),
        detail: hasWorkspace ? "Artifacts available for remediation and downstream automation." : "Generated guidance appears after workspace analysis.",
        tone: generatedArtifacts.length > 0 ? "positive" : "warning"
      }
    ],
    panels: [
      {
        title: "Active Findings",
        items: withFallbackItems(
          findings.map((finding) => `${finding.title}: ${finding.summary}`),
          createWorkspaceFallbackMessage(hasWorkspace, "No compliance findings were generated for the current workspace.")
        ),
        commandId: "architectureStudio.validateRegulations"
      },
      {
        title: "Remediation Focus",
        items: withFallbackItems(
          findings.map((finding) => `${finding.remediation.title}: ${finding.remediation.summary}`),
          createWorkspaceFallbackMessage(hasWorkspace, "No remediation actions are currently required for the workspace snapshot.")
        ),
        commandId: "architectureStudio.validateRegulations"
      }
    ]
  };
}

function createReportsSection(
  reports: readonly ReportArtifact[],
  generatedArtifacts: readonly GeneratedArtifact[],
  hasWorkspace: boolean
): DashboardSection {
  return {
    id: "reports",
    title: "Reports",
    description: "Show the reports and generated outputs that the extension can surface or publish for the workspace.",
    cards: [
      {
        title: "Reports",
        value: String(reports.length),
        detail: hasWorkspace ? "Report artifacts available from the current workspace snapshot." : "Open a workspace to generate live report artifacts.",
        tone: reports.length > 0 ? "positive" : "warning"
      },
      {
        title: "Generated Outputs",
        value: String(generatedArtifacts.length),
        detail: hasWorkspace ? "Project, documentation, AI, and report deliverables in scope." : "Workspace-driven deliverables appear here after analysis.",
        tone: generatedArtifacts.length > 0 ? "positive" : "warning"
      }
    ],
    panels: [
      {
        title: "Available Reports",
        items: withFallbackItems(
          reports.map((report) => `${report.title} (${report.format}) -> ${report.relativePath}`),
          createWorkspaceFallbackMessage(hasWorkspace, "No report artifacts were generated for the current workspace.")
        ),
        commandId: "architectureStudio.generateReports"
      },
      {
        title: "Generated Deliverables",
        items: withFallbackItems(
          generatedArtifacts.map((artifact) => `${artifact.title} -> ${artifact.relativePath}`),
          createWorkspaceFallbackMessage(hasWorkspace, "No generated deliverables are available for the current workspace snapshot.")
        ),
        commandId: "architectureStudio.generateAiInstructions"
      }
    ]
  };
}

function createRepositoryAnalysisSection(
  repositoryAnalysis: RepositoryAnalysisResult,
  findings: readonly FindingDefinition[],
  hasWorkspace: boolean
): DashboardSection {
  return {
    id: "repository-analysis",
    title: "Repository Analysis",
    description: "Inspect detected technologies, sensitive-data signals, evidence, and review work from one dashboard surface.",
    cards: [
      {
        title: "Detected Signals",
        value: String(repositoryAnalysis.signals.length),
        detail: hasWorkspace ? "Languages, frameworks, patterns, and delivery signals inferred from the workspace." : "Open a workspace to run repository analysis.",
        tone: repositoryAnalysis.signals.length > 0 ? "positive" : "warning"
      },
      {
        title: "Sensitive Data",
        value: String(repositoryAnalysis.sensitiveData.length),
        detail: hasWorkspace ? "Sensitive-data classifications inferred from repository evidence." : "Sensitive-data classifications appear after analysis.",
        tone: repositoryAnalysis.sensitiveData.length > 0 ? "warning" : "neutral"
      },
      {
        title: "Evidence References",
        value: String(countEvidence(findings)),
        detail: hasWorkspace ? "Captured files and paths supporting the current findings." : "Finding evidence is populated after analysis.",
        tone: countEvidence(findings) > 0 ? "warning" : "neutral"
      }
    ],
    panels: [
      {
        title: "Detected Stack",
        items: withFallbackItems(
          repositoryAnalysis.signals.map(
            (signal) => `${signal.label} (${signal.category}) - ${confidencePercentage(signal.confidence)} confidence`
          ),
          createWorkspaceFallbackMessage(hasWorkspace, "No repository technology signals were inferred for the current workspace.")
        ),
        commandId: "architectureStudio.analyzeRepository"
      },
      {
        title: "Sensitive Data",
        items: withFallbackItems(
          repositoryAnalysis.sensitiveData.map((classification) => {
            const evidence = classification.evidence[0] ?? "Repository evidence detected.";
            return `${classification.category} data - ${evidence}`;
          }),
          createWorkspaceFallbackMessage(hasWorkspace, "No sensitive-data classifications were detected for the current workspace.")
        ),
        commandId: "architectureStudio.analyzeRepository"
      },
      {
        title: "Review Queue",
        items: withFallbackItems(
          findings.map((finding) => `${finding.severity} - ${finding.title}`),
          createWorkspaceFallbackMessage(hasWorkspace, "No repository or compliance findings are waiting for review.")
        ),
        commandId: "architectureStudio.analyzeRepository"
      }
    ]
  };
}

export function createEmptySharedContractPayload(
  overrides: Partial<SharedContractPayload> = {}
): SharedContractPayload {
  return mergeSharedContractPayload(emptyPayload, overrides);
}

export function createSampleSharedContractPayload(
  overrides: Partial<SharedContractPayload> = {}
): SharedContractPayload {
  return mergeSharedContractPayload(samplePayload, overrides);
}

export function createDashboardState(
  payload: SharedContractPayload = createEmptySharedContractPayload(),
  options: DashboardStateOptions = {}
): DashboardState {
  const externalPackageStatuses = options.externalPackageStatuses ?? [];
  const hasWorkspace = options.hasWorkspace ?? Boolean(options.workspaceLabel);
  const repositoryAnalysis = options.repositoryAnalysis ?? emptyRepositoryAnalysis;
  const subtitle =
    options.subtitle
    ?? (options.workspaceLabel
      ? `Live workspace summary for ${options.workspaceLabel}.`
      : hasWorkspace
        ? "Live workspace summary for the active workspace."
        : "Open a workspace folder to load live Architecture Studio data.");

  return {
    generatedAt: new Date().toISOString(),
    title: "Architecture Studio",
    subtitle,
    sections: [
      createArchitectureSection(payload.graphNodes, payload.graphEdges, payload.projectSelection, hasWorkspace),
      createStandardsSection(payload.standards, payload.projectSelection, externalPackageStatuses, hasWorkspace),
      createComplianceSection(payload.complianceSummaries, payload.findings, payload.generatedArtifacts, hasWorkspace),
      createReportsSection(payload.reports, payload.generatedArtifacts, hasWorkspace),
      createRepositoryAnalysisSection(repositoryAnalysis, payload.findings, hasWorkspace)
    ]
  };
}

export function createDashboardStateMessage(state: DashboardState): DashboardHostMessage {
  return {
    type: "dashboard.stateChanged",
    state
  };
}

export function isDashboardWebviewMessage(value: unknown): value is DashboardWebviewMessage {
  if (!value || typeof value !== "object") {
    return false;
  }

  const candidate = value as Record<string, unknown>;

  if (candidate.type === "dashboard.ready") {
    return true;
  }

  return candidate.type === "dashboard.runCommand" && typeof candidate.commandId === "string";
}
