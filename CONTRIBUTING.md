# Contributing to Muxit

Muxit is a private, commercial project. The application core, SDK,
templates, and installer scripts in this repository are developed by
the Muxit team, and **we don't accept external pull requests for
those parts**.

The one place where outside contributions are welcomed and expected is
**drivers**.

## Writing a driver

Drivers are how Muxit talks to hardware, and the driver ecosystem is
intentionally open. This repository contains everything you need to
build one:

- [`sdk/`](sdk/) — the .NET 8 SDK that Tier 3 (DLL) drivers link
  against.
- [`templates/csharp/`](templates/csharp/) — a complete Tier 3 starter
  with a built-in test dashboard.
- [`templates/javascript/`](templates/javascript/) — a Tier 1
  (sandboxed JS) starter for simpler drivers.

Finished drivers are submitted as PRs to the
[muxit-io/driver-registry](https://github.com/muxit-io/driver-registry)
repository — not to this repo. See the README in each template folder
for the full workflow, and the registry's own README for submission
rules and licensing.

## Reporting bugs and asking questions

Even though the codebase isn't open for contributions, feedback is
very welcome:

- **Bugs and feature requests**: open an [issue](https://github.com/muxit-io/muxit/issues/new/choose)
  using one of the templates. If you've found a bug while using Muxit
  with specific hardware, please include the hardware model, firmware
  version, and how it's connected (USB, serial, network).
- **Questions, ideas, "how do I…"**: start a thread in
  [GitHub Discussions](https://github.com/muxit-io/muxit/discussions).
- **Security vulnerabilities**: see [SECURITY.md](SECURITY.md). Do
  **not** file a public issue for vulnerabilities.
