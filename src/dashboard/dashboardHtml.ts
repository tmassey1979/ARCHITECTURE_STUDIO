import type { DashboardSection, DashboardState } from "./dashboardState";

export interface DashboardResourceUri {
  toString(): string;
}

export interface DashboardUriApi {
  joinPath(base: DashboardResourceUri, ...paths: string[]): DashboardResourceUri;
}

export interface DashboardHtmlWebview {
  readonly cspSource: string;
  asWebviewUri(resource: DashboardResourceUri): DashboardResourceUri;
}

type RenderDashboardHtmlOptions = {
  readonly extensionUri: DashboardResourceUri;
  readonly nonce: string;
  readonly state: DashboardState;
  readonly uri: DashboardUriApi;
  readonly webview: DashboardHtmlWebview;
};

function escapeHtml(value: string): string {
  return value
    .replaceAll("&", "&amp;")
    .replaceAll("<", "&lt;")
    .replaceAll(">", "&gt;")
    .replaceAll('"', "&quot;")
    .replaceAll("'", "&#39;");
}

function renderSection(section: DashboardSection): string {
  const cards = section.cards
    .map(
      (card) => `
        <article class="summary-card summary-card--${card.tone}">
          <p class="summary-card__label">${escapeHtml(card.title)}</p>
          <p class="summary-card__value">${escapeHtml(card.value)}</p>
          <p class="summary-card__detail">${escapeHtml(card.detail)}</p>
        </article>
      `
    )
    .join("");

  const panels = section.panels
    .map((panel) => {
      const items = panel.items.map((item) => `<li>${escapeHtml(item)}</li>`).join("");
      const action = panel.commandId
        ? `<button class="panel-action" type="button" data-command-id="${escapeHtml(panel.commandId)}">Open</button>`
        : "";

      return `
        <article class="detail-panel">
          <header class="detail-panel__header">
            <h3>${escapeHtml(panel.title)}</h3>
            ${action}
          </header>
          <ul>${items}</ul>
        </article>
      `;
    })
    .join("");

  return `
    <section class="dashboard-section" data-section-id="${escapeHtml(section.id)}">
      <div class="dashboard-section__heading">
        <div>
          <p class="dashboard-section__eyebrow">${escapeHtml(section.title)}</p>
          <h2>${escapeHtml(section.title)}</h2>
        </div>
        <p class="dashboard-section__description">${escapeHtml(section.description)}</p>
      </div>
      <div class="summary-grid">${cards}</div>
      <div class="panel-grid">${panels}</div>
    </section>
  `;
}

export function renderDashboardHtml({
  extensionUri,
  nonce,
  state,
  uri,
  webview
}: RenderDashboardHtmlOptions): string {
  const styleUri = webview
    .asWebviewUri(uri.joinPath(extensionUri, "media", "dashboard", "dashboard.css"))
    .toString();
  const scriptUri = webview
    .asWebviewUri(uri.joinPath(extensionUri, "media", "dashboard", "dashboard.js"))
    .toString();
  const sectionsMarkup = state.sections.map((section) => renderSection(section)).join("");

  return `<!DOCTYPE html>
<html lang="en">
  <head>
    <meta charset="UTF-8" />
    <meta
      http-equiv="Content-Security-Policy"
      content="default-src 'none'; img-src ${webview.cspSource} data: https:; style-src ${webview.cspSource} 'unsafe-inline'; script-src ${webview.cspSource} 'nonce-${nonce}';"
    />
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
    <title>${escapeHtml(state.title)}</title>
    <link rel="stylesheet" href="${escapeHtml(styleUri)}" />
  </head>
  <body>
    <div class="dashboard-shell">
      <header class="hero">
        <div class="hero__copy">
          <p class="hero__eyebrow">Architecture Studio Dashboard</p>
          <h1>${escapeHtml(state.title)}</h1>
          <p class="hero__subtitle">${escapeHtml(state.subtitle)}</p>
        </div>
        <div class="hero__meta">
          <p class="hero__meta-label">Last generated</p>
          <p class="hero__meta-value" data-dashboard-generated-at>${escapeHtml(state.generatedAt)}</p>
        </div>
      </header>
      <main data-dashboard-root>
        ${sectionsMarkup}
      </main>
    </div>
    <script nonce="${escapeHtml(nonce)}" src="${escapeHtml(scriptUri)}"></script>
  </body>
</html>`;
}
