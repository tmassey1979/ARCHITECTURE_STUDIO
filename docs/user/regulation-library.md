# Regulation Library

## What It Is

Architecture Studio now ships with a broader regulation and control library that the compliance engine can evaluate directly.

This moves the plugin beyond a handful of seed examples and gives it a reusable content base for compliance scoring.

## Included Regulation Coverage

The current library includes modules for:

- GDPR
- CCPA
- COPPA
- HIPAA
- HITECH
- SOX
- PCI DSS
- ISO 27001
- NIST Cybersecurity Framework
- SOC 2
- TCPA
- CAN-SPAM
- PIPEDA

## Included Control Coverage

The control library covers the core controls called out in the product requirements, including:

- encryption
- audit logging
- role-based access control
- secrets management
- network segmentation
- data retention
- consent management

It also includes a broader supporting taxonomy so regulation scoring can be more realistic and extensible.

## Why It Matters

This library is what lets the compliance engine:

- determine supported regulation coverage consistently
- score the same repository inputs repeatably
- emit remediation findings from structured control definitions
- expand in later stories without rewriting the engine

## What Changes For Users

As the library expands, the compliance summaries and remediation output become more credible and more useful. The plugin can support a wider set of privacy, healthcare, financial, security, and communications frameworks from one data-driven foundation.

## Documentation Notes

This story is data and validation focused, so there are no new UI screenshots yet. The user-facing impact appears through richer compliance coverage in later dashboard and report stories.
