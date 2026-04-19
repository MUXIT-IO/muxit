// SPDX-License-Identifier: MIT
namespace Muxit.Driver.Sdk;

/// <summary>
/// Marks a driver assembly with a unique ID for per-driver licensing.
/// The host checks for this attribute and verifies the hash to detect tampering.
/// Drivers without this attribute are treated as free (no license needed).
/// </summary>
/// <example>
/// [assembly: DriverId("fairino", "abcdef1234...")]
/// </example>
[AttributeUsage(AttributeTargets.Assembly)]
public sealed class DriverIdAttribute : Attribute
{
    /// <summary>Driver identifier, e.g. "fairino".</summary>
    public string Id { get; }

    /// <summary>SHA256 hex of "muxit.io:driverId:{Id}" — tamper check.</summary>
    public string IdHash { get; }

    public DriverIdAttribute(string id, string idHash)
    {
        Id = id;
        IdHash = idHash;
    }
}
