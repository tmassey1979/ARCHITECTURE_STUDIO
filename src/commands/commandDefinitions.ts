export type ArchitectureStudioCommand = {
  readonly id: string;
  readonly title: string;
};

export const architectureStudioCommands: readonly ArchitectureStudioCommand[] = [
  { id: "architectureStudio.openDashboard", title: "Architecture Studio: Open Dashboard" },
  { id: "architectureStudio.composeStandards", title: "Architecture Studio: Compose Standards" },
  { id: "architectureStudio.analyzeRepository", title: "Architecture Studio: Analyze Repository" },
  { id: "architectureStudio.validateRegulations", title: "Architecture Studio: Validate Regulations" },
  { id: "architectureStudio.generateArchitecture", title: "Architecture Studio: Generate Architecture" },
  { id: "architectureStudio.generateProject", title: "Architecture Studio: Generate Project" },
  { id: "architectureStudio.generateReports", title: "Architecture Studio: Generate Reports" },
  { id: "architectureStudio.generateAiInstructions", title: "Architecture Studio: Generate AI Instructions" }
];
