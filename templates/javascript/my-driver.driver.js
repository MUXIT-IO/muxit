// SPDX-License-Identifier: MIT
/**
 * My Driver — Tier 1 (JavaScript) Muxit driver template.
 *
 * A self-contained example that exercises the full Tier 1 driver surface:
 * lifecycle (init / shutdown), properties (read + write), actions with and
 * without arguments, and a periodic streaming channel. All state is held in
 * memory so the driver runs without any real hardware.
 *
 * Copy this file, rename it to <your-driver>.driver.js, update the `meta`
 * block, and replace the implementation with your own logic. The file name
 * must match `manifest.entryPoint`.
 *
 * See docs/muxdriver-format.md (in muxit-io/driver-registry) for the
 * packaging spec.
 */
export default {
  meta: {
    name: "MyDriver",
    version: "0.1.0",
    group: "utilities",
    description:
      "Template Tier 1 driver exercising properties, actions, and streams. " +
      "Clone as a starting point for your own JavaScript driver.",
    properties: {
      label:       { type: "string",  access: "r/w", description: "Free-form device label" },
      count:       { type: "number",  access: "r/w", description: "Event counter" },
      enabled:     { type: "boolean", access: "r/w", description: "Whether the device is active" },
      temperature: { type: "number",  access: "r",   unit: "°C", description: "Current temperature reading" },
      uptime:      { type: "number",  access: "r",   unit: "s",  description: "Seconds since init" },
      status: {
        type: "object",
        access: "r",
        description: "Composite status snapshot",
        details:
          "Returns `{ label, enabled, count, temperature, uptime }`. Prefer " +
          "reading the individual properties when you only need one field — " +
          "this is mostly for dashboards that want a single round-trip.",
      },
    },
    actions: {
      reset: {
        description: "Reset all state to defaults",
        details: "Clears count, disables the device, restores the initial label.",
      },
      setLabel: {
        description: "Set the device label",
        args: { value: "string" },
      },
      calculate: {
        description: "Add two numbers and return the result",
        args: { a: "number", b: "number" },
      },
    },
    streams: ["tick"],
  },

  // ── Internal state ───────────────────────────────────────────────────
  _label: "My Device",
  _count: 0,
  _enabled: false,
  _initTime: 0,
  _streamEmitter: null,
  _streamTimer: null,

  // ── Lifecycle ────────────────────────────────────────────────────────

  /**
   * Called once at startup with the connector config. Open any connections,
   * start background tasks, and capture the stream emitter here.
   *
   * The host passes `_streamEmitter` on the driver object before calling
   * init, so you can use it from any method below.
   */
  async init(config) {
    this._label = config?.label ?? "My Device";
    this._count = 0;
    this._enabled = false;
    this._initTime = Date.now();

    // Emit a tick on the "tick" stream every second.
    this._streamTimer = setInterval(() => {
      if (!this._enabled || !this._streamEmitter) return;
      this._count++;
      this._streamEmitter("tick", {
        timestamp: new Date().toISOString(),
        count: this._count,
      });
    }, 1000);
  },

  /** Release resources. Called when the driver is torn down. */
  async shutdown() {
    if (this._streamTimer) clearInterval(this._streamTimer);
    this._streamTimer = null;
  },

  // ── Properties ───────────────────────────────────────────────────────

  async get(property) {
    switch (property) {
      case "label":       return this._label;
      case "count":       return this._count;
      case "enabled":     return this._enabled;
      case "temperature": return 22.5 + Math.sin(Date.now() / 5000) * 2;
      case "uptime":      return (Date.now() - this._initTime) / 1000;
      case "status":      return {
        label: this._label,
        enabled: this._enabled,
        count: this._count,
        temperature: await this.get("temperature"),
        uptime: await this.get("uptime"),
      };
      default:
        throw new Error(`Unknown property: ${property}`);
    }
  },

  async set(property, value) {
    switch (property) {
      case "label":   this._label = String(value); break;
      case "count":   this._count = Number(value); break;
      case "enabled": this._enabled = Boolean(value); break;
      default:
        throw new Error(`Unknown or read-only property: ${property}`);
    }
  },

  // ── Actions ──────────────────────────────────────────────────────────

  async execute(action, args) {
    switch (action) {
      case "reset":
        this._label = "My Device";
        this._count = 0;
        this._enabled = false;
        return "OK";

      case "setLabel": {
        const value = args?.value ?? "";
        this._label = String(value);
        return this._label;
      }

      case "calculate": {
        const a = Number(args?.a ?? 0);
        const b = Number(args?.b ?? 0);
        return a + b;
      }

      default:
        throw new Error(`Unknown action: ${action}`);
    }
  },
};
