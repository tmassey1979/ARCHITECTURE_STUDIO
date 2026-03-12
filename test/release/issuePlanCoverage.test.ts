import assert from "node:assert/strict";
import fs from "node:fs";
import path from "node:path";
import test from "node:test";

test("issue seed plan tracks the remaining codex follow-up stories", () => {
  const repoRoot = process.cwd();
  const issuePlan = fs.readFileSync(path.join(repoRoot, "codex/github-issues/issue-plan.md"), "utf8");
  const requiredStories = [
    "Feature: Expand the .NET CLI automation surface for command-line parity",
    "Feature: Replace placeholder dashboard projections with live workspace-backed data",
    "Feature: Implement PDF export for report generation"
  ];

  for (const storyTitle of requiredStories) {
    assert.match(issuePlan, new RegExp(storyTitle.replace(/[.*+?^${}()|[\\]\\\\]/g, "\\$&")));
  }
});
