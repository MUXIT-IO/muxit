# C# Driver Template (Tier 3)

A ready-to-build scaffold for a Muxit Tier 3 driver — a .NET 8 class library
that loads into the Muxit runtime as a DLL and links against `Muxit.Driver.Sdk`.

The included `TemplateDriver` is a fully functional example that exercises
every data type and feature of `IConnectorDriver`:

- all scalar, array, and object property types
- writable and read-only properties
- actions with no args, named args, and array args
- a periodic stream emitter
- a localhost HTTP dashboard (port 9999 by default) for hands-on testing

Nothing in it talks to real hardware, so you can build and run it on any
machine to confirm your toolchain works.

## Quick start

1. **Copy the template** into a new directory.

   ```sh
   cp -r templates/csharp my-device
   cd my-device
   ```

2. **Rename the files and namespaces** to match your driver:

   - Rename `Muxit.Driver.Template.csproj` to `MyDevice.csproj`.
   - Change `<AssemblyName>Template</AssemblyName>` to `<AssemblyName>MyDevice</AssemblyName>`
     (the assembly name becomes the DLL filename the runtime loads).
   - Change `<RootNamespace>` to match.
   - Rename `TemplateDriver.cs` and its class to `MyDeviceDriver`.

3. **Update the `ProjectReference`** in the `.csproj` to point at the SDK.
   If you're working inside this repo the relative path is already correct:

   ```xml
   <ProjectReference Include="..\..\sdk\Muxit.Driver.Sdk.csproj" />
   ```

   If you copied the template out of the repo, point it at wherever you
   placed `sdk/Muxit.Driver.Sdk.csproj`.

4. **Replace the schema and behavior.** Edit `GetProperties()`,
   `GetActions()`, `GetAsync`, `SetAsync`, and `ExecuteAsync` to match your
   device. The template is heavily commented to show the shape of each.

5. **Build.**

   ```sh
   dotnet publish -c Release
   ```

   The publish output under `bin/Release/net8.0/publish/` contains your
   `MyDevice.dll` and any dependency DLLs.

6. **Update `manifest.json`** with your `id` (format: `publisher/name`),
   display name, description, version, and `entryPoint` (the bare DLL
   filename, e.g. `MyDevice.dll`).

7. **Package into a `.muxdriver`.** Use the CLI in the driver-registry repo:

   ```sh
   node path/to/driver-registry/scripts/muxit-driver.js package \
     --manifest manifest.json \
     --entry bin/Release/net8.0/publish/MyDevice.dll \
     --deps bin/Release/net8.0/publish/
   ```

   Or assemble the ZIP by hand per [`docs/muxdriver-format.md`](https://github.com/muxit-io/driver-registry/blob/main/docs/muxdriver-format.md).

8. **Submit to the registry** by opening a PR against
   [muxit-io/driver-registry](https://github.com/muxit-io/driver-registry)
   with your driver's `drivers/<publisher>-<name>.json` entry.

## Testing locally

The template starts an HTTP dashboard on `http://localhost:9999/` during
`InitAsync`. Open it in a browser to read and write properties, fire
actions, and watch the log stream. Change the port by passing
`{ "dashboardPort": 8080 }` in your connector config.

## License

MIT — see [`../LICENSE`](../LICENSE).
