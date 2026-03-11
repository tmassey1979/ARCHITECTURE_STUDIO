import assert from "node:assert/strict";
import fs from "node:fs";
import path from "node:path";
import test from "node:test";

test("graph engine transport, dataset, and documentation artifacts exist", () => {
  const repoRoot = process.cwd();
  const requiredPaths = [
    "src/graph/technologyGraph.ts",
    "docs/developer/technology-graph.md",
    "docs/user/architecture-reasoning.md",
    "graph/datasets/frontend.yml",
    "graph/datasets/architecture.yml"
  ];

  for (const relativePath of requiredPaths) {
    assert.ok(fs.existsSync(path.join(repoRoot, relativePath)), `Expected ${relativePath} to exist.`);
  }
});
