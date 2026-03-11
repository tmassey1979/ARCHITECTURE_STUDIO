export type ArchitectureStudioCommand = {
  readonly id: string;
  readonly title: string;
  readonly handlerModule: string;
};

export const architectureStudioCommands: readonly ArchitectureStudioCommand[] = [
  {
    id: "architectureStudio.openDashboard",
    title: "Architecture Studio: Open Dashboard",
    handlerModule: "./handlers/openDashboardHandler"
  },
  {
    id: "architectureStudio.composeStandards",
    title: "Architecture Studio: Compose Standards",
    handlerModule: "./handlers/composeStandardsHandler"
  },
  {
    id: "architectureStudio.analyzeRepository",
    title: "Architecture Studio: Analyze Repository",
    handlerModule: "./handlers/analyzeRepositoryHandler"
  },
  {
    id: "architectureStudio.validateRegulations",
    title: "Architecture Studio: Validate Regulations",
    handlerModule: "./handlers/validateRegulationsHandler"
  },
  {
    id: "architectureStudio.generateArchitecture",
    title: "Architecture Studio: Generate Architecture",
    handlerModule: "./handlers/generateArchitectureHandler"
  },
  {
    id: "architectureStudio.generateProject",
    title: "Architecture Studio: Generate Project",
    handlerModule: "./handlers/generateProjectHandler"
  },
  {
    id: "architectureStudio.generateReports",
    title: "Architecture Studio: Generate Reports",
    handlerModule: "./handlers/generateReportsHandler"
  },
  {
    id: "architectureStudio.generateAiInstructions",
    title: "Architecture Studio: Generate AI Instructions",
    handlerModule: "./handlers/generateAiInstructionsHandler"
  }
];
