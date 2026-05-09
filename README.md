# Muxit

[![License: Apache 2.0](https://img.shields.io/badge/License-Apache_2.0-blue.svg)](LICENSE)
[![Status: beta](https://img.shields.io/badge/status-beta-orange.svg)](#status)
[![Docs](https://img.shields.io/badge/docs-docs.muxit.io-informational)](https://docs.muxit.io)

**Hardware orchestration for lab tinkerers.**

Control instruments, robots, sensors, and cameras through a unified
interface with automation scripts, real-time dashboards, and AI
integration.

## What it does

- **One interface for all your hardware** — Muxit talks to
  oscilloscopes, motion controllers, multimeters, cameras, sensors,
  GPIO, and custom rigs through a common driver model.
- **Live dashboards** — every connected device exposes typed properties
  and actions; build a dashboard in minutes, not weeks.
- **Automation scripts** — orchestrate sequences across multiple
  devices in JavaScript or via the HTTP/WebSocket API.
- **AI integration** — let an LLM read sensor values and drive actions,
  scoped to what you whitelist.
- **Open driver SDK** — write a driver for your own hardware in C#
  (Tier 3, full DLL) or JavaScript (Tier 1, sandboxed). See
  [`sdk/`](sdk/) and [`templates/`](templates/).

## Install

**Windows** (PowerShell):

```powershell
irm https://raw.githubusercontent.com/muxit-io/muxit/main/install.ps1 | iex
```

**Linux** (Ubuntu / Debian):

```bash
curl -fsSL https://raw.githubusercontent.com/muxit-io/muxit/main/install.sh | bash
```

After install, run `muxit` and open <http://127.0.0.1:8765>.

## Documentation

- **User docs**: <https://docs.muxit.io>
- **Driver SDK**: [`sdk/README.md`](sdk/README.md)
- **Driver templates**: [`templates/csharp/`](templates/csharp/) (Tier 3)
  and [`templates/javascript/`](templates/javascript/) (Tier 1)
- **Driver registry**: <https://github.com/muxit-io/driver-registry>

## Status

Muxit is currently in **beta**. The application works end-to-end and is
in active use, but APIs may still shift between minor releases. Pin
your driver SDK version against the runtime release notes.

## Community & support

- **Questions and ideas**: [GitHub Discussions](https://github.com/muxit-io/muxit/discussions)
- **Bugs**: [open an issue](https://github.com/muxit-io/muxit/issues/new/choose)
- **Security**: see [SECURITY.md](SECURITY.md) — do not file public
  issues for vulnerabilities.

## Contributing

See [CONTRIBUTING.md](CONTRIBUTING.md). The most useful contributions
are new drivers — start from [`templates/`](templates/) and submit
them to
[muxit-io/driver-registry](https://github.com/muxit-io/driver-registry).

## License

The contents of this repository — the Muxit Driver SDK, driver
templates, and installer scripts — are licensed under the Apache
License, Version 2.0 (see [LICENSE](LICENSE) and [NOTICE](NOTICE)).

The Muxit application itself (the binary distributed by the install
scripts) is proprietary software, governed by the Muxit End User
License Agreement which is presented and accepted on first launch.
