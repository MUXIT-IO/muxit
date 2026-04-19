// SPDX-License-Identifier: MIT
namespace Muxit.Driver.Sdk;

/// <summary>
/// Marks a driver assembly with its functional group for UI categorization.
/// If not present, the driver defaults to <see cref="DriverGroup.Instruments"/>.
/// </summary>
/// <example>
/// [assembly: DriverGroup(DriverGroup.Motion)]
/// </example>
[AttributeUsage(AttributeTargets.Assembly)]
public sealed class DriverGroupAttribute : Attribute
{
    public DriverGroup Group { get; }

    public DriverGroupAttribute(DriverGroup group)
    {
        Group = group;
    }
}
