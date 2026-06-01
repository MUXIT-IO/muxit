# Python Driver Template (Tier 2)

A ready-to-run scaffold for a Muxit Tier 2 driver — a single Python module
that the Muxit runtime runs as a subprocess, one per active connector.
Good when your device is easiest to reach from Python: numpy / scipy
pipelines, `pyvisa`, vendor wheels, ML models (`torch`, `transformers`), or
anything with a mature Python binding.

Tier 2 drivers run **subprocess-isolated** in their own virtual environment
— heavier than a Tier 1 JS driver but lighter than a Tier 3 DLL, and they
get the whole Python ecosystem. They are always **free** (the subprocess
host doesn't support licensed entitlements).

The included `my-driver.driver.py` exercises the full Tier 2 surface:
lifecycle, properties (read + write), actions with args, structured logging,
and a streaming channel. It holds all state in memory, so it builds and runs
with no real hardware.

> **Not sure you need a driver at all?** If your device speaks SCPI, prints
> text lines, or emits binary frames, you don't write a driver — you write a
> short connector config that uses a built-in **Protocol** and let the AI
> draft it. See the [Driver, Protocol, Engine](https://docs.muxit.io/reference/concepts)
> guide first. Reach for this template when a Protocol can't express your device.

## Quick start

1. **Copy the template** into a new directory.

   ```sh
   cp -r templates/python my-driver
   cd my-driver
   ```

2. **Rename the driver file.** The filename must match `manifest.entryPoint`.

   ```sh
   mv my-driver.driver.py scope-probe.driver.py
   ```

3. **Edit the driver.** Update the `META` block (name, description,
   properties, actions, streams) and replace the
   `init`/`get`/`set`/`execute`/`shutdown` bodies with your own logic. Keep
   heavy imports inside `init()`.

4. **Declare runtime dependencies** (optional) in a `requirements.txt` next
   to the driver — one package per line, standard pip syntax. Muxit creates
   a per-driver virtual environment and `pip install`s it on first
   activation; pip output streams to the connector console so you can watch
   it install.

5. **Update `manifest.json`** with your `id` (format: `publisher/name`),
   display name, description, version, and `entryPoint` (must match the
   driver filename). `tier` is `2` and `category` must be `free`.

6. **Package into a `.muxdriver`.** Use the CLI in the driver-registry repo:

   ```sh
   node path/to/driver-registry/scripts/muxit-driver.js package \
     --manifest manifest.json \
     --entry scope-probe.driver.py
   ```

   The packager vendors the `muxit_driver` SDK into the package
   automatically — you don't ship a copy. See
   [`docs/muxdriver-format.md`](https://github.com/muxit-io/driver-registry/blob/main/docs/muxdriver-format.md)
   for the full spec.

7. **Submit to the registry** by opening a PR against
   [muxit-io/driver-registry](https://github.com/muxit-io/driver-registry)
   with your driver's `drivers/<publisher>-<name>.json` entry.

## Driver shape

Subclass `muxit_driver.Driver`, set `META` as a class attribute, and
override the methods you need:

| Method                       | When called                                              |
|------------------------------|----------------------------------------------------------|
| `init(config)`               | Once when the connector activates.                       |
| `get(property)`              | Each time a property is read.                            |
| `set(property, value)`       | Each time a writable property is assigned.               |
| `execute(action, args)`      | Each time an action is invoked.                          |
| `shutdown()`                 | Once before the subprocess exits.                        |

Any method may be sync or `async`. Two helpers are available on `self`:

- `self.log(message, level="info")` — structured log line, forwarded to the
  connector console.
- `self.emit(stream, data)` — push a value on one of the streams listed in
  `META["streams"]`.

Finish the file with the standard entry point:

```python
if __name__ == "__main__":
    run(MyDriver)
```

Requires Python 3.10+ on the host. Muxit probes `MUXIT_PYTHON`, then
`python3`, then `python`; if no interpreter is found, Python drivers are
skipped at scan time (the rest of Muxit still boots).

Full reference: <https://docs.muxit.io/reference/driver-sdk-python>.

## License

Apache-2.0 — see [`../LICENSE`](../LICENSE).
