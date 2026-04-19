# JavaScript Driver Template (Tier 1)

A ready-to-run scaffold for a Muxit Tier 1 driver — a single JavaScript
module that the Muxit runtime loads into a sandboxed V8 worker. No compile
step, no dependencies.

Tier 1 drivers are the easiest way to ship to the Muxit registry. They run
with no filesystem, network, or process access beyond what the host API
exposes, which means users can install them from unknown authors without
the security review that Tier 3 DLLs require.

The included `my-driver.driver.js` exercises the full Tier 1 surface:
lifecycle, properties (read + write), actions with args, and a streaming
channel.

## Quick start

1. **Copy the template** into a new directory.

   ```sh
   cp -r templates/javascript my-driver
   cd my-driver
   ```

2. **Rename the driver file.** The filename must match `manifest.entryPoint`.

   ```sh
   mv my-driver.driver.js scope-probe.driver.js
   ```

3. **Edit the driver.** Update the `meta` block with your driver's name,
   description, properties, actions, and streams, then replace the
   `init`/`get`/`set`/`execute`/`shutdown` bodies with your own logic.

4. **Update `manifest.json`** with your `id` (format: `publisher/name`),
   display name, description, version, and `entryPoint` (must match the
   driver filename).

5. **Package into a `.muxdriver`.** Use the CLI in the driver-registry repo:

   ```sh
   node path/to/driver-registry/scripts/muxit-driver.js package \
     --manifest manifest.json \
     --entry scope-probe.driver.js
   ```

   A Tier 1 package is just a ZIP of the two files (`manifest.json` plus
   the driver) — any tool that produces a conformant ZIP will work. See
   [`docs/muxdriver-format.md`](https://github.com/muxit-io/driver-registry/blob/main/docs/muxdriver-format.md)
   for the full spec.

6. **Submit to the registry** by opening a PR against
   [muxit-io/driver-registry](https://github.com/muxit-io/driver-registry)
   with your driver's `drivers/<publisher>-<name>.json` entry.

## Driver shape

The default export is a plain object with six methods:

| Method                       | When called                                              |
|------------------------------|----------------------------------------------------------|
| `init(config)`               | Once at startup, with the connector's config object.     |
| `get(property)`              | Each time the host or a connector reads a property.      |
| `set(property, value)`       | Each time a writable property is assigned.               |
| `execute(action, args)`      | Each time an action is invoked.                          |
| `shutdown()`                 | Once when the driver is torn down.                       |

Plus a `meta` object describing the driver's name, version, group,
properties, actions, and streams.

The host assigns a function to `this._streamEmitter` before calling `init`.
Call `this._streamEmitter("<stream-name>", payload)` to push data on one
of the streams listed in `meta.streams`.

## License

MIT — see [`../LICENSE`](../LICENSE).
