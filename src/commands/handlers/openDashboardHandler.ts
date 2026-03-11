import type { StudioCommandHandler } from "../commandRuntime";

const openDashboardHandler: StudioCommandHandler = async ({ services }) => {
  if (!services.showDashboard) {
    throw new Error("Dashboard services are not configured.");
  }

  await services.showDashboard();
};

export default openDashboardHandler;
