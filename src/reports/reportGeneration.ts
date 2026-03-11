import type { ReportArtifact } from "../contracts/sharedContracts";

export type GeneratedReportFile = {
  readonly relativePath: string;
  readonly format: ReportArtifact["format"];
  readonly content: string;
};

export type ReportGenerationResult = {
  readonly reportArtifacts: readonly ReportArtifact[];
  readonly files: readonly GeneratedReportFile[];
  readonly pdfFallbackUsed: boolean;
};
