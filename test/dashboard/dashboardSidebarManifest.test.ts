import assert from "node:assert/strict";
import fs from "node:fs";
import path from "node:path";
import test from "node:test";

test("package manifest contributes the Architecture Studio activity bar container and sidebar view", () => {
  const manifest = JSON.parse(fs.readFileSync(path.join(process.cwd(), "package.json"), "utf8")) as {
    activationEvents: string[];
    capabilities?: {
      untrustedWorkspaces?: {
        supported?: boolean | "limited";
        description?: string;
      };
    };
    contributes: {
      viewsContainers?: {
        activitybar?: Array<{ id: string; title: string; icon: string }>;
      };
      views?: Record<string, Array<{ id: string; name: string; type?: string }>>;
    };
  };

  const activityBarContainers = manifest.contributes.viewsContainers?.activitybar ?? [];
  const architectureStudioContainer = activityBarContainers.find((container) => container.id === "architectureStudio");
  assert.ok(architectureStudioContainer);
  assert.equal(architectureStudioContainer.title, "Architecture Studio");
  assert.equal(architectureStudioContainer.icon, "media/architecture-studio.svg");

  const contributedViews = manifest.contributes.views?.architectureStudio ?? [];
  assert.deepEqual(contributedViews, [
    {
      id: "architectureStudio.dashboardView",
      name: "Dashboard",
      type: "webview"
    }
  ]);

  assert.ok(manifest.activationEvents.includes("onView:architectureStudio.dashboardView"));
  assert.equal(manifest.capabilities?.untrustedWorkspaces?.supported, true);
  assert.match(
    manifest.capabilities?.untrustedWorkspaces?.description ?? "",
    /restricted mode|workspace trust|untrusted workspace/i
  );
  assert.ok(fs.existsSync(path.join(process.cwd(), "media", "architecture-studio.svg")));
});
