import assert from "node:assert/strict";
import test from "node:test";

import {
  architectureStudioDashboardViewId,
  registerArchitectureStudioDashboardSidebar
} from "../../src/dashboard/registerDashboardSidebar";

type Disposable = { dispose(): void };

test("registerArchitectureStudioDashboardSidebar wires the dashboard sidebar provider into the contributed view id", () => {
  const subscriptions: Disposable[] = [];
  let registeredViewId = "";
  const provider = {
    resolveWebviewView() {}
  };

  registerArchitectureStudioDashboardSidebar(
    {
      subscriptions
    } as never,
    {
      registerWebviewViewProvider(viewId, registeredProvider) {
        registeredViewId = viewId;
        assert.equal(registeredProvider, provider);
        return { dispose() {} };
      }
    },
    provider
  );

  assert.equal(registeredViewId, architectureStudioDashboardViewId);
  assert.equal(subscriptions.length, 1);
});
