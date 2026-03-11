import assert from "node:assert/strict";
import fs from "node:fs";
import path from "node:path";
import test from "node:test";

test("report generation transport and documentation artifacts exist", () => {
  const repoRoot = process.cwd();
  const requiredPaths = [
    "src/reports/reportGeneration.ts",
    "docs/developer/report-generation.md",
    "docs/user/report-export.md"
  ];

  for (const relativePath of requiredPaths) {
    assert.ok(fs.existsSync(path.join(repoRoot, relativePath)), `Expected ${relativePath} to exist.`);
  }
});
