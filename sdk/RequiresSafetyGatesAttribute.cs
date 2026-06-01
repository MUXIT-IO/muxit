// SPDX-License-Identifier: Apache-2.0
namespace Muxit.Driver.Sdk;

/// <summary>
/// Marks a driver assembly as either requiring the Muxit safety gate
/// (limits, confirmations, rate caps, audit log) or opting out of it.
/// Set to <c>false</c> only for drivers that have no path to physical
/// hardware and no destructive actions — e.g. webcams, file I/O, IP cameras.
/// If not present, the driver defaults to <c>true</c> (gated).
///
/// Note: only officially-signed DLLs are permitted to opt out. On an
/// unsigned third-party DLL the loader logs a warning and forces gates
/// back on, so a community driver cannot disable the safety system.
/// </summary>
/// <example>
/// [assembly: RequiresSafetyGates(false)]
/// </example>
[AttributeUsage(AttributeTargets.Assembly)]
public sealed class RequiresSafetyGatesAttribute : Attribute
{
    public bool RequiresSafetyGates { get; }

    public RequiresSafetyGatesAttribute(bool requiresSafetyGates)
    {
        RequiresSafetyGates = requiresSafetyGates;
    }
}
