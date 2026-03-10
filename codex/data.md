# Architecture Studio Codex Datasets

This is a combined Codex canvas with three datasets for the Architecture Studio platform. These datasets are meant to dramatically improve Codex's ability to generate the full extension automatically.

---

## 1. Technology Graph Dataset (~1,500 nodes)

**Purpose:** Represents all common programming languages, frameworks, libraries, architecture patterns, databases, cloud services, CI/CD tools, and infrastructure components.

**Format:** YAML nodes with relationships.

Example snippet:

```yaml
nodes:
  - id: react
    category: frontend
    requires: ["javascript", "npm"]
    pairs_with: ["redux", "react-router"]
    recommended_with: ["rest api", "graphql"]

  - id: angular
    category: frontend
    requires: ["typescript", "npm"]
    pairs_with: ["rxjs"]
    recommended_with: ["rest api"]

  - id: aspnet-core
    category: backend
    requires: ["c#", ".net"]
    pairs_with: ["entity-framework", "sql-server"]
    recommended_with: ["cqrs"]

  - id: cqrs
    category: architecture
    requires: ["event-sourcing"]
    pairs_with: ["aspnet-core"]

  - id: docker
    category: devops
    pairs_with: ["kubernetes", "jenkins"]
    recommended_with: ["helm"]

  - id: kubernetes
    category: devops
    requires: ["docker"]
    pairs_with: ["helm", "prometheus"]
```

**Scope:** Expand to include ~1,500 nodes covering:

* Frontend frameworks (React Native, Vue, Flutter, WPF, Blazor)
* Backend frameworks (Spring, Django, Node.js, FastAPI, Go)
* Cloud services (AWS, Azure, GCP)
* Databases (SQL, NoSQL, NewSQL)
* CI/CD tools (GitHub Actions, GitLab CI, Jenkins, Bamboo, CircleCI)
* Messaging (Kafka, RabbitMQ)
* Observability (Prometheus, Grafana, OpenTelemetry)
* Security tools (Vault, Keycloak, OAuth providers)
* Architecture patterns (Microservices, CQRS, Event Sourcing, DDD)

---

## 2. Regulation & Compliance Dataset (~200 controls)

**Purpose:** Represent laws, standards, and regulations including required controls.

**Format:** YAML modules

Example snippet:

```yaml
regulations:
  - id: hipaa
    category: healthcare
    jurisdiction: US
    required_controls:
      - encryption_at_rest
      - encryption_in_transit
      - audit_logging
      - access_control
      - breach_notification
    data_types: [protected_health_information]

  - id: gdpr
    category: privacy
    jurisdiction: EU
    required_controls:
      - data_minimization
      - consent_tracking
      - right_to_delete
      - data_portability
      - breach_notification
    data_types: [personal_data]

  - id: pci_dss
    category: financial
    jurisdiction: global
    required_controls:
      - card_data_encryption
      - network_segmentation
      - access_logging
      - vulnerability_scanning
```

**Scope:** Expand to ~200 controls including:

* Privacy laws (GDPR, CCPA, COPPA, PIPEDA)
* Healthcare laws (HIPAA, HITECH)
* Financial laws (SOX, PCI DSS)
* Messaging/communication laws (TCPA, CAN-SPAM)
* Security frameworks (ISO 27001, NIST CSF, SOC2)

---

## 3. Project Templates Dataset (20+ architectures)

**Purpose:** Provide boilerplate structures, CI/CD pipelines, infrastructure manifests, and AGENTS.md templates.

**Format:** JSON / Markdown

Example snippet:

```json
{
  "template_id": "react_cqrs_k8s",
  "frontend": "react",
  "backend": "aspnet-core",
  "architecture": "cqrs",
  "infrastructure": ["docker", "kubernetes", "helm"],
  "ci_cd": ["github-actions"],
  "compliance": ["hipaa"],
  "structure": {
    "src": ["frontend", "backend"],
    "docs": ["architecture.md", "AGENTS.md"],
    "docker": ["Dockerfile", "docker-compose.yml"],
    "k8s": ["deployment.yaml", "service.yaml"],
    ".github/workflows": ["ci.yml"]
  }
}
```

**Scope:** Include ~20 templates covering:

* React + REST + CQRS
* React Native + GraphQL
* WPF + .NET backend
* ASP.NET + Microservices
* Angular + Event Sourcing
* Multi-cloud (AWS + GCP + Azure)
* Compliance-aware setups (HIPAA, PCI DSS, GDPR)
* DevOps variations (Docker only, Docker + K8s, Helm, Jenkins pipelines, GitLab CI, GitHub Actions)

---

## Notes for Codex

1. Use these datasets to reason about architecture, compliance, and code generation.
2. Enforce regulation and control validation in all generated projects.
3. Generate dashboard, reporting, and AGENTS.md automatically for every new template.
4. Include graph reasoning to recommend technologies automatically.
5. Maintain modularity for future expansion of technologies, regulations, and templates.

---

# END OF CANVAS

This combined canvas provides the **full knowledge base** that Codex can use to generate the complete VS Code Architecture Studio platform with compliance, reporting, and AI-assisted project generation.
