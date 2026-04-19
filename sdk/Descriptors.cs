// SPDX-License-Identifier: MIT
namespace Muxit.Driver.Sdk;

/// <summary>
/// Functional grouping for drivers. Tells users what kind of device/service a driver works with.
/// </summary>
public enum DriverGroup
{
    /// <summary>Test &amp; measurement devices and cameras: power supplies, oscilloscopes, spectrometers, webcams, IP cameras.</summary>
    Instruments,
    /// <summary>Robots, CNC, plotters, stages — anything that moves.</summary>
    Motion,
    /// <summary>MQTT, serial monitors, protocol bridges.</summary>
    Communication,
    /// <summary>File access, test device, GUI — software-only, no hardware.</summary>
    Utilities,
}

/// <summary>
/// Describes a driver property.
/// </summary>
/// <param name="Name">Property name (used in GetAsync/SetAsync calls).</param>
/// <param name="Type">Type string: "string", "int", "double", "bool", "double[]", "int[]", "string[]", "object".</param>
/// <param name="Access">"R" (read-only), "W" (write-only), or "R/W" (read-write).</param>
/// <param name="Unit">Optional unit label (e.g., "°C", "mm/s", "nm").</param>
/// <param name="Description">Optional one-line human-readable description (always-on; appears in the AI system prompt summary).</param>
/// <param name="Details">Optional markdown with extended documentation (parameter enums, side effects, failure modes, truncation caps). Surfaced on-demand via get_connector_schema / get_driver_schema; never included in the upfront AI prompt summary.</param>
public record PropertyDescriptor(
    string Name, string Type, string Access,
    string Unit = "", string Description = "", string Details = "");

/// <summary>
/// Describes a single action argument.
/// </summary>
/// <param name="Name">Argument name (used as key in the args dictionary).</param>
/// <param name="Type">Type string: "string", "int", "double", "bool", "double[]", "int[]", "string[]", "object".</param>
/// <param name="Description">Optional human-readable description of what this argument controls.</param>
public record ArgDescriptor(
    string Name, string Type, string Description = "");

/// <summary>
/// Describes a driver action.
/// </summary>
/// <param name="Name">Action name (used in ExecuteAsync calls).</param>
/// <param name="Description">Optional one-line human-readable description (always-on; appears in the AI system prompt summary).</param>
/// <param name="Args">Optional list of argument descriptors with name, type, and description.</param>
/// <param name="Details">Optional markdown with extended documentation (parameter enums, side effects, failure modes, truncation caps). Surfaced on-demand via get_connector_schema / get_driver_schema; never included in the upfront AI prompt summary.</param>
public record ActionDescriptor(
    string Name, string Description = "",
    List<ArgDescriptor>? Args = null,
    string Details = "");
