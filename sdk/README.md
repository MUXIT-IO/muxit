# Muxit.Driver.Sdk

The public contract that Tier 3 (.NET DLL) Muxit drivers link against.

A Tier 3 driver is a .NET 8 class library that implements `IConnectorDriver`
and is loaded by the Muxit runtime at install time. This project provides
the interfaces, attributes, descriptors, and helper types needed to build
one — nothing more.

## Contents

| File | Purpose |
|------|---------|
| `IConnectorDriver.cs`     | Main lifecycle interface drivers implement. |
| `IDriverHost.cs`          | Scoped pub/sub handed to drivers by the host. |
| `Descriptors.cs`          | `PropertyDescriptor`, `ActionDescriptor`, `ArgDescriptor`, `DriverGroup`. |
| `DriverConfig.cs`         | Safe typed helpers for config and action args. |
| `DriverGroupAttribute.cs` | Assembly attribute: functional group (instruments, motion, …). |
| `DriverIdAttribute.cs`    | Assembly attribute: optional per-driver identifier. |
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

MIT — see [`LICENSE`](./LICENSE).
