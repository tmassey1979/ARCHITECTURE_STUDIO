import fs from "node:fs";
import path from "node:path";
import { spawn } from "node:child_process";

import type { RepositoryAnalysisResult } from "../analysis/repositoryAnalysis";
import type { ComposedStandardsResult } from "../standards/standardsComposition";
import type { WorkspaceArchitectureEvaluationResult } from "../graph/technologyGraph";
import type { ComplianceEvaluationResult } from "../compliance/complianceEvaluation";
import type { ProjectSelectionProfile } from "../contracts/sharedContracts";
import type { ProjectGenerationResult } from "../generators/projectGeneration";
import type { ReportGenerationResult } from "../reports/reportGeneration";
import type {
  AiInstructionGenerationRequest,
  AiInstructionGenerationResult
} from "../ai/aiInstructionGeneration";

type OutputLike = {
  appendLine(line: string): void;
};

type CoreCliProcessRequest = {
  readonly command: string;
  readonly args: readonly string[];
  readonly cwd: string;
  readonly stdin?: string;
};

type CoreCliProcessResult = {
  readonly exitCode: number;
  readonly stdout: string;
  readonly stderr: string;
};

export interface ArchitectureStudioCoreCli {
  analyzeRepository(workspacePath: string): Promise<RepositoryAnalysisResult>;
  composeStandards(workspacePath: string): Promise<ComposedStandardsResult>;
  evaluateArchitecture(workspacePath: string): Promise<WorkspaceArchitectureEvaluationResult>;
  validateRegulations(workspacePath: string): Promise<ComplianceEvaluationResult>;
  inferProjectSelection(workspacePath: string): Promise<ProjectSelectionProfile>;
  generateProject(selection: ProjectSelectionProfile): Promise<ProjectGenerationResult>;
  generateReports(workspacePath: string): Promise<ReportGenerationResult>;
  buildAiInstructionRequest(workspacePath: string): Promise<AiInstructionGenerationRequest>;
  generateAiInstructions(request: AiInstructionGenerationRequest): Promise<AiInstructionGenerationResult>;
}

type CreateArchitectureStudioCoreCliOptions = {
  readonly extensionPath: string;
  readonly fileExists?: (candidatePath: string) => boolean;
  readonly output?: OutputLike;
  readonly runProcess?: (request: CoreCliProcessRequest) => Promise<CoreCliProcessResult>;
};

type CoreCliLaunchPlan = {
  readonly command: string;
  readonly args: readonly string[];
};

function defaultRunProcess({
  command,
  args,
  cwd,
  stdin
}: CoreCliProcessRequest): Promise<CoreCliProcessResult> {
  return new Promise((resolve, reject) => {
    const child = spawn(command, args, {
      cwd,
      stdio: "pipe"
    });

    let stdout = "";
    let stderr = "";

    child.stdout.on("data", (chunk) => {
      stdout += chunk.toString();
    });
    child.stderr.on("data", (chunk) => {
      stderr += chunk.toString();
    });
    child.on("error", reject);
    child.on("close", (exitCode) => {
      resolve({
        exitCode: exitCode ?? -1,
        stdout,
        stderr
      });
    });

    if (stdin) {
      child.stdin.write(stdin);
    }

    child.stdin.end();
  });
}

function resolveLaunchPlan(
  extensionPath: string,
  fileExists: (candidatePath: string) => boolean = fs.existsSync
): CoreCliLaunchPlan {
  const publishedDllPath = path.join(extensionPath, "core-host", "ArchitectureStudio.Cli.dll");

  if (fileExists(publishedDllPath)) {
    return {
      command: "dotnet",
      args: [publishedDllPath]
    };
  }

  return {
    command: "dotnet",
    args: [
      "run",
      "--project",
      path.join(extensionPath, "core", "ArchitectureStudio.Cli", "ArchitectureStudio.Cli.csproj"),
      "--no-build",
      "--"
    ]
  };
}

async function invokeCoreCli<TResult>(
  launchPlan: CoreCliLaunchPlan,
  extensionPath: string,
  commandName: string,
  output: OutputLike | undefined,
  runProcess: (request: CoreCliProcessRequest) => Promise<CoreCliProcessResult>,
  options: {
    readonly workspacePath?: string;
    readonly stdinJson?: string;
  } = {}
): Promise<TResult> {
  const args = [...launchPlan.args, commandName];

  if (options.workspacePath) {
    args.push("--workspace", options.workspacePath);
  }

  if (options.stdinJson) {
    args.push("--stdin");
  }

  output?.appendLine(`[Architecture Studio] Core CLI command: ${commandName}`);
  const result = await runProcess({
    command: launchPlan.command,
    args,
    cwd: extensionPath,
    stdin: options.stdinJson
  });

  if (result.exitCode !== 0) {
    throw new Error(result.stderr.trim() || `Core CLI command '${commandName}' failed with exit code ${result.exitCode}.`);
  }

  if (!result.stdout.trim()) {
    throw new Error(`Core CLI command '${commandName}' produced no JSON output.`);
  }

  return JSON.parse(result.stdout) as TResult;
}

export function createArchitectureStudioCoreCli({
  extensionPath,
  fileExists,
  output,
  runProcess = defaultRunProcess
}: CreateArchitectureStudioCoreCliOptions): ArchitectureStudioCoreCli {
  const launchPlan = resolveLaunchPlan(extensionPath, fileExists);

  return {
    analyzeRepository(workspacePath) {
      return invokeCoreCli<RepositoryAnalysisResult>(launchPlan, extensionPath, "analyze-repository", output, runProcess, {
        workspacePath
      });
    },
    composeStandards(workspacePath) {
      return invokeCoreCli<ComposedStandardsResult>(launchPlan, extensionPath, "compose-standards", output, runProcess, {
        workspacePath
      });
    },
    evaluateArchitecture(workspacePath) {
      return invokeCoreCli<WorkspaceArchitectureEvaluationResult>(
        launchPlan,
        extensionPath,
        "evaluate-architecture",
        output,
        runProcess,
        {
          workspacePath
        }
      );
    },
    validateRegulations(workspacePath) {
      return invokeCoreCli<ComplianceEvaluationResult>(
        launchPlan,
        extensionPath,
        "validate-regulations",
        output,
        runProcess,
        {
          workspacePath
        }
      );
    },
    inferProjectSelection(workspacePath) {
      return invokeCoreCli<ProjectSelectionProfile>(
        launchPlan,
        extensionPath,
        "infer-project-selection",
        output,
        runProcess,
        {
          workspacePath
        }
      );
    },
    generateProject(selection) {
      return invokeCoreCli<ProjectGenerationResult>(launchPlan, extensionPath, "generate-project", output, runProcess, {
        stdinJson: JSON.stringify(selection)
      });
    },
    generateReports(workspacePath) {
      return invokeCoreCli<ReportGenerationResult>(launchPlan, extensionPath, "generate-reports", output, runProcess, {
        workspacePath
      });
    },
    buildAiInstructionRequest(workspacePath) {
      return invokeCoreCli<AiInstructionGenerationRequest>(
        launchPlan,
        extensionPath,
        "build-ai-instruction-request",
        output,
        runProcess,
        {
          workspacePath
        }
      );
    },
    generateAiInstructions(request) {
      return invokeCoreCli<AiInstructionGenerationResult>(
        launchPlan,
        extensionPath,
        "generate-ai-instructions",
        output,
        runProcess,
        {
          stdinJson: JSON.stringify(request)
        }
      );
    }
  };
}
