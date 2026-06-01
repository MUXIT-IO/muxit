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
- **Built-in protocols + AI** — most custom hardware needs no driver at
  all. If your device speaks SCPI, prints text lines, or emits binary
  frames, pick a built-in protocol (`Scpi` / `LineText` / `BinaryStream`)
  and let the AI assistant draft and test the connector against your
  hardware. See the [docs](https://docs.muxit.io/reference/concepts).
- **Open driver SDK** — when a protocol can't express your device (vendor
  SDKs, native libraries, Python-only stacks), write a driver in JavaScript
  (Tier 1, sandboxed), Python (Tier 2, subprocess-isolated), or C# (Tier 3,
  full DLL). See [`sdk/`](sdk/) and [`templates/`](templates/).

## Install

**Windows** (PowerShell):

```powershell
irm https://raw.githubusercontent.com/muxit-io/muxit/main/install.ps1 | iex
```

**Linux** (Ubuntu / Debian, x86_64 or ARM64):

```bash
curl -fsSL https://raw.githubusercontent.com/muxit-io/muxit/main/install.sh | bash
```

The installer auto-detects your CPU architecture and pulls the matching
build. Supported targets:

- **x86_64** — Intel / AMD desktops and servers (tested on Ubuntu
  22.04 LTS and 24.04 LTS).
- **aarch64 / arm64** — Raspberry Pi 4 / 5 on 64-bit Pi OS, and other
  arm64 single-board computers. Available from **v0.32.0** onward.
  32-bit Pi OS (`armv7l`) is not supported — reflash with the 64-bit
  image.

After install, run `muxit` and open <http://127.0.0.1:8765>.

## Documentation

- **User docs**: <https://docs.muxit.io>
- **Driver SDK**: [`sdk/README.md`](sdk/README.md)
- **Driver templates**: [`templates/javascript/`](templates/javascript/)
  (Tier 1), [`templates/python/`](templates/python/) (Tier 2), and
  [`templates/csharp/`](templates/csharp/) (Tier 3)
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
