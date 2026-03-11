export const repositorySignalCategories = [
  "Language",
  "Framework",
  "ArchitecturePattern",
  "Infrastructure",
  "CiCd"
] as const;

export const sensitiveDataCategories = ["Personal", "Financial", "Health", "ChildData"] as const;

export type RepositorySignalCategory = (typeof repositorySignalCategories)[number];
export type SensitiveDataCategory = (typeof sensitiveDataCategories)[number];

export type RepositorySignal = {
  readonly id: string;
  readonly label: string;
  readonly category: RepositorySignalCategory;
  readonly confidence: number;
  readonly evidence: readonly string[];
  readonly affectedPaths: readonly string[];
};

export type SensitiveDataClassification = {
  readonly category: SensitiveDataCategory;
  readonly confidence: number;
  readonly evidence: readonly string[];
  readonly affectedPaths: readonly string[];
};

export type RepositoryAnalysisResult = {
  readonly signals: readonly RepositorySignal[];
  readonly sensitiveData: readonly SensitiveDataClassification[];
};
