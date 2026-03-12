# Issue 20 CLI Automation Parity

- replaced the ad hoc CLI parser with a structured `System.CommandLine` command surface
- added .NET tests for help text, JSON command execution, invalid usage, BOM-prefixed standard input, and packaged-host smoke flows
- documented direct CLI automation usage for both developers and users
- preserved the existing Architecture Studio workflow verbs while making help, exit codes, and error behavior automation-friendly
