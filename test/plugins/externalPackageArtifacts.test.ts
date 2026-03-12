import assert from "node:assert/strict";
import fs from "node:fs";
import path from "node:path";
import test from "node:test";

test("external package contract and documentation artifacts exist", () => {
  const repoRoot = process.cwd();
  const requiredPaths = [
    "src/plugins/externalPackages.ts",
    "docs/developer/external-packages.md",
    "docs/user/external-packages.md",
    "plugins/packs/aws-architecture-pack/architecture-studio.package.json",
    "plugins/packs/kafka-event-driven-pack/architecture-studio.package.json",
    "plugins/packs/banking-compliance-pack/architecture-studio.package.json"
  ];

  for (const relativePath of requiredPaths) {
    assert.ok(fs.existsSync(path.join(repoRoot, relativePath)), `Expected ${relativePath} to exist.`);
  }
});
