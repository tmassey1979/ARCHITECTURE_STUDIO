import assert from "node:assert/strict";
import fs from "node:fs";
import path from "node:path";
import test from "node:test";

test("standards engine transport and documentation artifacts exist", () => {
  const repoRoot = process.cwd();
  const requiredPaths = [
    "src/standards/standardsComposition.ts",
    "docs/developer/standards-engine.md",
    "docs/user/standards-composition.md",
    "standards/packages/architecture-studio.seed.json"
  ];

  for (const relativePath of requiredPaths) {
    assert.ok(fs.existsSync(path.join(repoRoot, relativePath)), `Expected ${relativePath} to exist.`);
  }
});
