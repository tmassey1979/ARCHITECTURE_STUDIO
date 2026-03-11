import assert from "node:assert/strict";
import fs from "node:fs";
import path from "node:path";
import test from "node:test";

test("dashboard webview assets and documentation exist in repo paths that package with the extension", () => {
  const repoRoot = process.cwd();
  const requiredPaths = [
    "media/dashboard/dashboard.css",
    "media/dashboard/dashboard.js",
    "docs/developer/dashboard-webview.md",
    "docs/user/dashboard.md"
  ];

  for (const relativePath of requiredPaths) {
    assert.ok(fs.existsSync(path.join(repoRoot, relativePath)), `Expected ${relativePath} to exist.`);
  }
});
