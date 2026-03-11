import assert from "node:assert/strict";
import fs from "node:fs";
import path from "node:path";
import test from "node:test";

test("generator transport, documentation, and template artifacts exist", () => {
  const repoRoot = process.cwd();
  const requiredPaths = [
    "src/generators/projectGeneration.ts",
    "docs/developer/generation-engine.md",
    "docs/user/project-generation.md",
    "templates/projects/frontend/react.json",
    "templates/projects/backend/aspnet-core.json",
    "templates/projects/architecture/clean-architecture.json",
    "templates/projects/compliance/hipaa.json",
    "templates/pipelines/github-actions.json",
    "templates/pipelines/gitlab-ci.json",
    "templates/pipelines/jenkins.json",
    "templates/pipelines/azure-devops.json",
    "templates/pipelines/circleci.json",
    "templates/infra/docker.json",
    "templates/infra/kubernetes.json"
  ];

  for (const relativePath of requiredPaths) {
    assert.ok(fs.existsSync(path.join(repoRoot, relativePath)), `Expected ${relativePath} to exist.`);
  }
});
