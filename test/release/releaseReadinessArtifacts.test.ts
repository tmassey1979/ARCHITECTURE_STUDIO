import assert from "node:assert/strict";
import fs from "node:fs";
import path from "node:path";
import test from "node:test";

test("release readiness documentation and smoke fixture artifacts exist", () => {
  const repoRoot = process.cwd();
  const requiredPaths = [
    "docs/developer/release-readiness.md",
    "fixtures/sample-workspaces/fintech-platform/README.md",
    "fixtures/sample-workspaces/fintech-platform/.github/workflows/ci.yml",
    "fixtures/sample-workspaces/fintech-platform/src/Web/appsettings.json"
  ];

  for (const relativePath of requiredPaths) {
    assert.ok(fs.existsSync(path.join(repoRoot, relativePath)), `Expected ${relativePath} to exist.`);
  }
});
