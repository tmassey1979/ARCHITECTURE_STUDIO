import assert from "node:assert/strict";
import fs from "node:fs";
import path from "node:path";
import test from "node:test";

test("compliance library modules and documentation artifacts exist", () => {
  const repoRoot = process.cwd();
  const requiredPaths = [
    "compliance/controls/library.json",
    "compliance/regulations/gdpr.json",
    "compliance/regulations/ccpa.json",
    "compliance/regulations/coppa.json",
    "compliance/regulations/hipaa.json",
    "compliance/regulations/hitech.json",
    "compliance/regulations/sox.json",
    "compliance/regulations/pci-dss.json",
    "compliance/regulations/iso-27001.json",
    "compliance/regulations/nist-csf.json",
    "compliance/regulations/soc2.json",
    "compliance/regulations/tcpa.json",
    "compliance/regulations/can-spam.json",
    "compliance/regulations/pipeda.json",
    "docs/developer/compliance-library.md",
    "docs/user/regulation-library.md"
  ];

  for (const relativePath of requiredPaths) {
    assert.ok(fs.existsSync(path.join(repoRoot, relativePath)), `Expected ${relativePath} to exist.`);
  }
});
