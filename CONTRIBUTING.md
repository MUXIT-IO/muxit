# Contributing to Muxit

Thanks for your interest in Muxit. There are several ways to contribute,
and you don't have to be a .NET developer to help.

## Where to ask questions

- **General questions, ideas, "how do I…"**: open a thread in
  [GitHub Discussions](https://github.com/muxit-io/muxit/discussions).
- **Bugs and feature requests**: open an [issue](https://github.com/muxit-io/muxit/issues/new/choose)
  using one of the templates.
- **Security vulnerabilities**: see [SECURITY.md](SECURITY.md). Do
  **not** file a public issue.

## Writing a driver

Most contributions to Muxit's hardware coverage take the form of
**drivers** rather than core changes. The repository contains everything
you need to build one:

- [`sdk/`](sdk/) — the .NET 8 SDK that Tier 3 (DLL) drivers link
  against.
- [`templates/csharp/`](templates/csharp/) — a complete Tier 3 starter
  with a built-in test dashboard.
- [`templates/javascript/`](templates/javascript/) — a Tier 1
  (sandboxed JS) starter for simpler drivers.

Finished drivers are submitted as PRs to the
[muxit-io/driver-registry](https://github.com/muxit-io/driver-registry)
repository. See the README in each template folder for the full
workflow.

## Pull requests to this repository

For changes to the SDK, templates, or installer scripts:

1. If the change is non-trivial, open an issue or discussion first so
   we can agree on the direction before code is written.
2. Fork the repo and create a topic branch from `main`.
3. Keep PRs small and focused on a single concern.
4. Use the PR template — describe what changed and how to test it.
5. By submitting a PR, you agree that your contribution is licensed
   under the [Apache License 2.0](LICENSE).

## Code style

- C# code in `sdk/` and `templates/csharp/` follows the default .NET
  formatting (`dotnet format`).
- Shell and PowerShell scripts in the root follow the existing style
  in `install.sh` and `install.ps1`: explicit error handling, no
  silent failures.

## Reporting hardware issues

If you've found a bug while using Muxit with specific hardware, please
include the hardware model, firmware version, and how it's connected
(USB, serial port, network) in the issue. The bug report template
prompts for this — fill it in as completely as you can.
