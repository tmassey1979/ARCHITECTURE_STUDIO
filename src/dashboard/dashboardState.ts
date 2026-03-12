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

const placeholderPayload: SharedContractPayload = {
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
      title: "Secrets scanning placeholder finding",
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
      title: "Control mapping placeholder finding",
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
  selection?: Partial<ProjectSelectionProfile>
): ProjectSelectionProfile {
  if (!selection) {
    return placeholderPayload.projectSelection;
  }

  return {
    ...placeholderPayload.projectSelection,
    ...selection,
    ciCd: selection.ciCd ?? placeholderPayload.projectSelection.ciCd,
    infrastructure: selection.infrastructure ?? placeholderPayload.projectSelection.infrastructure,
    complianceTargets: selection.complianceTargets ?? placeholderPayload.projectSelection.complianceTargets
  };
}

export function createPlaceholderSharedContractPayload(
  overrides: Partial<SharedContractPayload> = {}
): SharedContractPayload {
  return {
    ...placeholderPayload,
    ...overrides,
    standards: overrides.standards ?? placeholderPayload.standards,
    regulations: overrides.regulations ?? placeholderPayload.regulations,
    controls: overrides.controls ?? placeholderPayload.controls,
    graphNodes: overrides.graphNodes ?? placeholderPayload.graphNodes,
    graphEdges: overrides.graphEdges ?? placeholderPayload.graphEdges,
    complianceSummaries: overrides.complianceSummaries ?? placeholderPayload.complianceSummaries,
    findings: overrides.findings ?? placeholderPayload.findings,
    reports: overrides.reports ?? placeholderPayload.reports,
    generatedArtifacts: overrides.generatedArtifacts ?? placeholderPayload.generatedArtifacts,
    projectSelection: mergeProjectSelection(overrides.projectSelection)
  };
}

function countEvidence(findings: readonly FindingDefinition[]): number {
  return findings.reduce((total, finding) => total + (finding.evidence?.length ?? 0), 0);
}

function severityCount(findings: readonly FindingDefinition[], severity: FindingDefinition["severity"]): number {
  return findings.filter((finding) => finding.severity === severity).length;
}

function createArchitectureSection(
  graphNodes: readonly GraphNodeDefinition[],
  graphEdges: readonly GraphEdgeDefinition[],
  selection: ProjectSelectionProfile
): DashboardSection {
  return {
    id: "architecture",
    title: "Architecture",
    description: "Review the active architecture model, graph relationships, and current build target stack.",
    cards: [
      {
        title: "Graph Nodes",
        value: String(graphNodes.length),
        detail: "Technologies, patterns, and controls currently represented.",
        tone: "neutral"
      },
      {
        title: "Graph Edges",
        value: String(graphEdges.length),
        detail: "Known compatibility and dependency relationships.",
        tone: "positive"
      },
      {
        title: "Target Pattern",
        value: selection.architecturePattern,
        detail: `${selection.frontend} with ${selection.backend}.`,
        tone: "neutral"
      }
    ],
    panels: [
      {
        title: "Primary Components",
        items: graphNodes.map((node) => `${node.label} (${node.category})`),
        commandId: "architectureStudio.generateArchitecture"
      },
      {
        title: "Delivery Stack",
        items: [
          `Frontend: ${selection.frontend}`,
          `Backend: ${selection.backend}`,
          `Infrastructure: ${selection.infrastructure.join(", ")}`
        ],
        commandId: "architectureStudio.generateProject"
      }
    ]
  };
}

function createStandardsSection(
  standards: readonly StandardDefinition[],
  selection: ProjectSelectionProfile,
  externalPackageStatuses: readonly ExternalPackageLoadStatus[]
): DashboardSection {
  const externalPackCount = externalPackageStatuses.length;
  const externalPackItems =
    externalPackageStatuses.length > 0
      ? externalPackageStatuses.map((status) => {
          const contributionKinds =
            status.contributionKinds.length > 0 ? ` [${status.contributionKinds.join(", ")}]` : "";
          return `${status.packageId} - ${status.status}: ${status.message}${contributionKinds}`;
        })
      : ["No external packages discovered yet."];

  return {
    id: "standards",
    title: "Standards",
    description: "Track the baseline standards package that should inform architecture and generation decisions.",
    cards: [
      {
        title: "Active Standards",
        value: String(standards.length),
        detail: "Curated standards available to compose into solution guidance.",
        tone: "positive"
      },
      {
        title: "Compliance Targets",
        value: String(selection.complianceTargets.length),
        detail: selection.complianceTargets.join(", "),
        tone: "neutral"
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
        items: standards.map((standard) => `${standard.title}: ${standard.summary}`),
        commandId: "architectureStudio.composeStandards"
      },
      {
        title: "External Package Status",
        items: externalPackItems,
        commandId: "architectureStudio.composeStandards"
      }
    ]
  };
}

function createComplianceSection(
  complianceSummaries: readonly ComplianceSummary[],
  findings: readonly FindingDefinition[],
  generatedArtifacts: readonly GeneratedArtifact[]
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
            title: "Critical Findings",
            value: String(severityCount(findings, "Critical")),
            detail: "Immediate attention items in the current analysis set.",
            tone: "critical" as const
          },
          {
            title: "High Findings",
            value: String(severityCount(findings, "High")),
            detail: "Important issues that should be remediated early in delivery.",
            tone: "warning" as const
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
        detail: "Artifacts that can support remediation and downstream automation.",
        tone: "positive"
      }
    ],
    panels: [
      {
        title: "Active Findings",
        items: findings.map((finding) => `${finding.title}: ${finding.summary}`),
        commandId: "architectureStudio.validateRegulations"
      },
      {
        title: "Remediation Focus",
        items: findings.map((finding) => `${finding.remediation.title}: ${finding.remediation.summary}`),
        commandId: "architectureStudio.validateRegulations"
      }
    ]
  };
}

function createReportsSection(
  reports: readonly ReportArtifact[],
  generatedArtifacts: readonly GeneratedArtifact[]
): DashboardSection {
  return {
    id: "reports",
    title: "Reports",
    description: "Show the reports and generated outputs that the extension will surface or publish.",
    cards: [
      {
        title: "Reports",
        value: String(reports.length),
        detail: "Versioned report artifacts currently mapped in the shared contracts.",
        tone: "neutral"
      },
      {
        title: "Generated Outputs",
        value: String(generatedArtifacts.length),
        detail: "Project, documentation, and AI instruction assets ready for downstream flows.",
        tone: "positive"
      }
    ],
    panels: [
      {
        title: "Available Reports",
        items: reports.map((report) => `${report.title} (${report.format}) -> ${report.relativePath}`),
        commandId: "architectureStudio.generateReports"
      },
      {
        title: "Generated Deliverables",
        items: generatedArtifacts.map((artifact) => `${artifact.title} -> ${artifact.relativePath}`),
        commandId: "architectureStudio.generateAiInstructions"
      }
    ]
  };
}

function createRepositoryAnalysisSection(findings: readonly FindingDefinition[]): DashboardSection {
  return {
    id: "repository-analysis",
    title: "Repository Analysis",
    description: "Inspect repository findings, evidence, and placeholder analysis coverage from one dashboard surface.",
    cards: [
      {
        title: "Findings",
        value: String(findings.length),
        detail: "Repository and compliance findings available to inspect.",
        tone: "neutral"
      },
      {
        title: "Evidence References",
        value: String(countEvidence(findings)),
        detail: "Captured files and paths supporting the current findings.",
        tone: "warning"
      }
    ],
    panels: [
      {
        title: "Evidence Trail",
        items: findings.flatMap((finding) =>
          (finding.evidence ?? []).map((evidence) => `${finding.title}: ${evidence}`)
        ),
        commandId: "architectureStudio.analyzeRepository"
      },
      {
        title: "Review Queue",
        items: findings.map((finding) => `${finding.severity} - ${finding.title}`),
        commandId: "architectureStudio.analyzeRepository"
      }
    ]
  };
}

export function createDashboardState(
  payload: SharedContractPayload = createPlaceholderSharedContractPayload(),
  options: DashboardStateOptions = {}
): DashboardState {
  const externalPackageStatuses = options.externalPackageStatuses ?? [];

  return {
    generatedAt: new Date().toISOString(),
    title: "Architecture Studio",
    subtitle: "Architecture, standards, compliance, reporting, and repository analysis in one command surface.",
    sections: [
      createArchitectureSection(payload.graphNodes, payload.graphEdges, payload.projectSelection),
      createStandardsSection(payload.standards, payload.projectSelection, externalPackageStatuses),
      createComplianceSection(payload.complianceSummaries, payload.findings, payload.generatedArtifacts),
      createReportsSection(payload.reports, payload.generatedArtifacts),
      createRepositoryAnalysisSection(payload.findings)
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
