# Muxit.Driver.Sdk

The public contract that Tier 3 (.NET DLL) Muxit drivers link against.

A Tier 3 driver is a .NET 8.0 class library that implements `IConnectorDriver`
and is loaded by the Muxit runtime (a .NET 9 host) at install time. This
project provides the interfaces, attributes, descriptors, and helper types
needed to build one — nothing more.

> **This SDK is only for Tier 3 (C# DLL) drivers.** Tier 1 (JavaScript) and
> Tier 2 (Python) drivers don't link against it — a JS driver exports a plain
> object, and a Python driver subclasses `muxit_driver.Driver`. See the
> [`templates/`](../templates) for all three.
>
> **And most devices don't need a driver at all.** If your hardware speaks
> SCPI, prints text lines, or emits binary frames, use a built-in
> [Protocol](https://docs.muxit.io/reference/concepts) and a short connector
> config instead. Reach for this SDK when a Protocol can't express your
> device — a vendor SDK, a native library, or quirky multi-step logic.

## Contents

| File | Purpose |
|------|---------|
| `IConnectorDriver.cs`     | Main lifecycle interface drivers implement. |
| `IDriverHost.cs`          | Scoped pub/sub handed to drivers by the host. |
| `Descriptors.cs`          | `PropertyDescriptor`, `ActionDescriptor`, `ArgDescriptor`, `DriverGroup`. |
| `DriverConfig.cs`         | Safe typed helpers for config and action args. |
| `DriverGroupAttribute.cs` | Assembly attribute: functional group (instruments, motion, …). |
| `DriverIdAttribute.cs`    | Assembly attribute: optional per-driver identifier. |
| `RequiresSafetyGatesAttribute.cs` | Assembly attribute: opt out of the safety gate (signed DLLs only). |
| `AudioFrameInfo.cs`       | PCM format metadata for the host's audio stream emitter. |
| `Muxit.Driver.Sdk.csproj` | Project file. Targets `net8.0`. |

## Usage

From a driver project, add a project reference to this SDK:

```xml
<ItemGroup>
  <ProjectReference Include="path/to/sdk/Muxit.Driver.Sdk.csproj" />
</ItemGroup>
```

See [`../templates/csharp`](../templates/csharp) for a complete working
example, and [`docs/muxdriver-format.md`](https://github.com/muxit-io/driver-registry/blob/main/docs/muxdriver-format.md)
in the driver-registry for the package and registry entry spec.

## Versioning

The SDK's public surface is locked to the Muxit runtime release it ships
with. A Tier 3 driver built against SDK X runs on any Muxit runtime that
advertises compatibility with X (see the `minMuxitVersion` field of the
registry entry). Breaking changes are rare and are announced in the Muxit
release notes.

## License

Apache-2.0 — see [`LICENSE`](./LICENSE).
