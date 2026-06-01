# SPDX-License-Identifier: Apache-2.0
"""My Driver — Tier 2 (Python) Muxit driver template.

A self-contained example that exercises the full Tier 2 driver surface:
lifecycle (init / shutdown), properties (read + write), actions with and
without arguments, structured logging, and a periodic streaming channel.
All state is held in memory so the driver runs without any real hardware.

Copy this file, rename it to ``<your-driver>.driver.py``, update the
``META`` block, and replace the method bodies with your own logic. The
file name must match ``manifest.entryPoint``.

Each connector that uses this driver runs in its own Python subprocess and
talks to the Muxit host over line-delimited JSON-RPC on stdin/stdout — the
``muxit_driver`` SDK (vendored into the .muxdriver at package time) handles
that wire protocol for you.

IMPORTANT: keep heavy imports (numpy, torch, vendor SDKs, …) *inside*
``init`` — never at module level — so the host's scan-time ``--scan`` pass
stays fast and works without the driver's runtime dependencies installed.
List those runtime deps in a sibling ``requirements.txt``; Muxit installs
them into a per-driver virtual environment on first activation.
"""

from __future__ import annotations

import threading
import time
from typing import Any

from muxit_driver import Driver, run


class MyDriver(Driver):
    # ── Schema ────────────────────────────────────────────────────────────
    # Declared once at class level so `--scan` can read it without
    # instantiating the driver. `access` is "R", "W", or "R/W".
    META: dict[str, Any] = {
        "name": "MyDriver",
        "version": "0.1.0",
        "group": "utilities",  # instruments | motion | communication | utilities
        "description": (
            "Template Tier 2 driver exercising properties, actions, and a "
            "stream. Clone as a starting point for your own Python driver."
        ),
        "properties": {
            "label":       {"type": "string", "access": "R/W", "description": "Free-form device label"},
            "count":       {"type": "int",    "access": "R/W", "description": "Event counter"},
            "enabled":     {"type": "bool",   "access": "R/W", "description": "Whether the device is active"},
            "temperature": {"type": "double", "access": "R", "unit": "C", "description": "Current temperature reading"},
            "uptime":      {"type": "double", "access": "R", "unit": "s", "description": "Seconds since init"},
        },
        "actions": {
            "reset":     {"description": "Reset all state to defaults",
                          "details": "Clears count, disables the device, restores the initial label."},
            "set_label": {"description": "Set the device label", "args": {"value": "string"}},
            "calculate": {"description": "Add two numbers and return the result",
                          "args": {"a": "double", "b": "double"}},
        },
        "streams": ["tick"],
    }

    # ── Lifecycle ─────────────────────────────────────────────────────────

    def init(self, config: dict[str, Any] | None) -> None:
        """Called once when the connector activates. Open connections,
        load models, start background tasks."""
        cfg = config or {}
        self._label: str = cfg.get("label", "My Device")
        self._count: int = 0
        self._enabled: bool = False
        self._init_time: float = time.monotonic()
        self._stop = threading.Event()

        self.log("MyDriver initialised")

        # Emit a tick on the "tick" stream every second. A typed Driver
        # subclass can stream via self.emit(stream, data); the generic
        # `Python` driver cannot.
        self._thread = threading.Thread(target=self._tick_loop, daemon=True)
        self._thread.start()

    def shutdown(self) -> None:
        """Called once before the subprocess exits. Release resources."""
        self._stop.set()

    def _tick_loop(self) -> None:
        while not self._stop.wait(1.0):
            if not self._enabled:
                continue
            self._count += 1
            self.emit("tick", {"timestamp": time.time(), "count": self._count})

    # ── Properties ────────────────────────────────────────────────────────

    def get(self, property: str) -> Any:
        if property == "label":
            return self._label
        if property == "count":
            return self._count
        if property == "enabled":
            return self._enabled
        if property == "temperature":
            # Pretend hardware: a slow sine around 22.5 °C.
            return 22.5 + 2.0 * __import__("math").sin(time.monotonic() / 5.0)
        if property == "uptime":
            return time.monotonic() - self._init_time
        raise KeyError(f"Unknown property: {property}")

    def set(self, property: str, value: Any) -> None:
        if property == "label":
            self._label = str(value)
        elif property == "count":
            self._count = int(value)
        elif property == "enabled":
            self._enabled = bool(value)
        else:
            raise KeyError(f"Unknown or read-only property: {property}")

    # ── Actions ───────────────────────────────────────────────────────────

    def execute(self, action: str, args: dict[str, Any] | None) -> Any:
        args = args or {}
        if action == "reset":
            self._label = "My Device"
            self._count = 0
            self._enabled = False
            return "OK"
        if action == "set_label":
            self._label = str(args.get("value", ""))
            return self._label
        if action == "calculate":
            return float(args.get("a", 0)) + float(args.get("b", 0))
        raise KeyError(f"Unknown action: {action}")


if __name__ == "__main__":
    run(MyDriver)
