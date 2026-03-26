# Muxit

**Hardware orchestration for lab tinkerers.**

Control instruments, robots, sensors, and cameras through a unified interface with automation scripts, real-time dashboards, and AI integration.

## Quick Install (Windows)

```powershell
irm https://raw.githubusercontent.com/lampmaker/muxit.io/main/install.ps1 | iex
```

This downloads the latest release to `%LOCALAPPDATA%\muxit` and adds it to your PATH.

## Manual Install

1. Download the latest `muxit-win-x64.zip` from [Releases](https://github.com/lampmaker/muxit.io/releases/latest)
2. Extract to a folder of your choice
3. Run `MuxitServer.exe`

## Usage

```bash
muxit                  # Start server (http://localhost:8765)
muxit --gui            # Start server + open browser
muxit --cli            # Interactive CLI for driver testing
muxit --mcp            # MCP server mode (for Claude, etc.)
muxit --version        # Show version
muxit update           # Update to latest version
```

Open **http://localhost:8765** in your browser to access the dashboard.

## What's Included

- **Self-contained binary** — no .NET or Node.js required
- **Web dashboard** — drag-and-drop widgets with live data
- **Script editor** — Monaco editor with sandboxed JavaScript automation
- **Built-in drivers** — Webcam, ONVIF cameras, MQTT, file access, test device
- **Community drivers** — SCPI instruments, G-code/GRBL, serial monitor, pen plotters
- **Premium drivers** — Fairino robot arm, Avantes spectrometer (license required)
- **AI integration** — Chat, voice control, autonomous agents, MCP server
- **Documentation** — built-in at `/docs/`

## Premium Drivers

Premium drivers are included in every download but require a license key to activate.
Without a key, they appear in the driver list but won't load.

Activate via the web UI (Settings > License) or CLI:
```bash
muxit --cli
> license activate <your-key>
```

## Updating

Run the install script again, or use the built-in update command:

```bash
muxit update
```

Your workspace (scripts, connectors, dashboards, configs) is preserved during updates.

## System Requirements

- Windows 10/11 (x64)
- ~100 MB disk space
- A modern browser (Chrome, Edge, Firefox)

## Links

- [Documentation](https://muxit.io) (coming soon)
- [Report Issues](https://github.com/lampmaker/muxit.io/issues)
