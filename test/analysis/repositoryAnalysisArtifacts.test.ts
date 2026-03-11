import assert from "node:assert/strict";
import fs from "node:fs";
import path from "node:path";
import test from "node:test";

test("repository analysis transport and documentation artifacts exist", () => {
  const repoRoot = process.cwd();
  const requiredPaths = [
    "src/analysis/repositoryAnalysis.ts",
    "docs/developer/repository-analysis.md",
    "docs/user/repository-analysis.md"
  ];

  for (const relativePath of requiredPaths) {
    assert.ok(fs.existsSync(path.join(repoRoot, relativePath)), `Expected ${relativePath} to exist.`);
  }
});
