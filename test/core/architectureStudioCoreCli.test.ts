import assert from "node:assert/strict";
import test from "node:test";

import { createArchitectureStudioCoreCli } from "../../src/core/architectureStudioCoreCli";

test("core CLI bridge sends project generation requests over standard input without an extra stdin flag", async () => {
  const invocations: {
    readonly args: readonly string[];
    readonly stdin?: string;
  }[] = [];
  const cli = createArchitectureStudioCoreCli({
    extensionPath: "C:/code/Playground/ARCHITECTURE_STUDIO",
    fileExists() {
      return true;
    },
    async runProcess(request) {
      invocations.push({
        args: request.args,
        stdin: request.stdin
      });

      return {
        exitCode: 0,
        stdout: JSON.stringify({
          templateIds: ["infra-aws-reference-stack"],
          generatedArtifacts: [],
          files: []
        }),
        stderr: ""
      };
    }
  });

  const result = await cli.generateProject({
    frontend: "react",
    backend: "aspnet-core",
    architecturePattern: "event-driven",
    ciCd: ["github-actions"],
    infrastructure: ["aws"],
    complianceTargets: []
  });

  assert.deepEqual(result.templateIds, ["infra-aws-reference-stack"]);
  assert.equal(invocations.length, 1);
  assert.equal(invocations[0].args[1], "generate-project");
  assert.ok(!invocations[0].args.includes("--stdin"));
  assert.match(invocations[0].stdin ?? "", /"architecturePattern":"event-driven"/);
});

test("core CLI bridge sends AI instruction generation requests over standard input without an extra stdin flag", async () => {
  const invocations: {
    readonly args: readonly string[];
    readonly stdin?: string;
  }[] = [];
  const cli = createArchitectureStudioCoreCli({
    extensionPath: "C:/code/Playground/ARCHITECTURE_STUDIO",
    fileExists() {
      return true;
    },
    async runProcess(request) {
      invocations.push({
        args: request.args,
        stdin: request.stdin
      });

      return {
        exitCode: 0,
        stdout: JSON.stringify({
          generatedArtifacts: [],
          files: []
        }),
        stderr: ""
      };
    }
  });

  await cli.generateAiInstructions({
    projectName: "Architecture Studio",
    targetKind: "AnalyzedRepository",
    projectSelection: {
      frontend: "react",
      backend: "aspnet-core",
      architecturePattern: "clean-architecture",
      ciCd: ["github-actions"],
      infrastructure: ["docker"],
      complianceTargets: ["pci-dss"]
    },
    standards: [],
    complianceSummaries: [],
    findings: []
  });

  assert.equal(invocations.length, 1);
  assert.equal(invocations[0].args[1], "generate-ai-instructions");
  assert.ok(!invocations[0].args.includes("--stdin"));
  assert.match(invocations[0].stdin ?? "", /"projectName":"Architecture Studio"/);
});
