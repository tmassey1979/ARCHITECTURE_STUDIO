/* global Element, acquireVsCodeApi, document, window */

(function () {
  const vscode =
    typeof acquireVsCodeApi === "function"
      ? acquireVsCodeApi()
      : {
          getState() {
            return undefined;
          },
          postMessage() {},
          setState() {}
        };

  function escapeHtml(value) {
    return String(value)
      .replaceAll("&", "&amp;")
      .replaceAll("<", "&lt;")
      .replaceAll(">", "&gt;")
      .replaceAll('"', "&quot;")
      .replaceAll("'", "&#39;");
  }

  function isStateMessage(message) {
    return Boolean(
      message &&
        typeof message === "object" &&
        message.type === "dashboard.stateChanged" &&
        message.state &&
        Array.isArray(message.state.sections)
    );
  }

  function renderSection(section) {
    const cards = section.cards
      .map(
        (card) => `
          <article class="summary-card summary-card--${escapeHtml(card.tone)}">
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
        const button = panel.commandId
          ? `<button class="panel-action" type="button" data-command-id="${escapeHtml(panel.commandId)}">Open</button>`
          : "";

        return `
          <article class="detail-panel">
            <header class="detail-panel__header">
              <h3>${escapeHtml(panel.title)}</h3>
              ${button}
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

  function render(state) {
    const root = document.querySelector("[data-dashboard-root]");
    const generatedAt = document.querySelector("[data-dashboard-generated-at]");

    if (!root) {
      return;
    }

    root.innerHTML = state.sections.map(renderSection).join("");

    if (generatedAt) {
      generatedAt.textContent = state.generatedAt;
    }

    vscode.setState(state);
  }

  document.addEventListener("click", (event) => {
    const target = event.target;

    if (!(target instanceof Element)) {
      return;
    }

    const action = target.closest("[data-command-id]");

    if (!action) {
      return;
    }

    const commandId = action.getAttribute("data-command-id");

    if (!commandId) {
      return;
    }

    vscode.postMessage({
      type: "dashboard.runCommand",
      commandId
    });
  });

  window.addEventListener("message", (event) => {
    if (isStateMessage(event.data)) {
      render(event.data.state);
    }
  });

  const cachedState = vscode.getState();

  if (cachedState && Array.isArray(cachedState.sections)) {
    render(cachedState);
  }

  vscode.postMessage({ type: "dashboard.ready" });
})();
