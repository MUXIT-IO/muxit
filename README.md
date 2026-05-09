# Muxit

**Hardware orchestration for lab tinkerers.**

Control instruments, robots, sensors, and cameras through a unified interface with automation scripts, real-time dashboards, and AI integration.

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

## License

The contents of this repository — the Muxit Driver SDK, driver templates,
and installer scripts — are licensed under the Apache License, Version 2.0
(see [LICENSE](LICENSE) and [NOTICE](NOTICE)).

The Muxit application itself (the binary distributed by the install scripts)
is proprietary software, governed by the Muxit End User License Agreement
which is presented and accepted on first launch.
