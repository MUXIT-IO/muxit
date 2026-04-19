// SPDX-License-Identifier: MIT
namespace Muxit.Driver.Sdk;

/// <summary>
/// Interface that all Muxit device drivers must implement.
///
/// Lifecycle:
///   1. Constructor (must be parameterless, no I/O — driver is instantiated via reflection)
///   2. InitAsync — called with config; open connections here
///   3. GetAsync / SetAsync / ExecuteAsync — called during normal operation
///   4. ShutdownAsync — close connections, release resources
///
/// All values passed to and from the driver are native C# types:
///   string, int, double, bool, object[], Dictionary&lt;string, object?&gt;, etc.
///   Drivers never need to handle JsonElement or any serialization format.
/// </summary>
public interface IConnectorDriver
{
    /// <summary>Display name for this driver (used in logging and UI).</summary>
    string Name { get; }

    /// <summary>Optional version string (e.g., "1.0.0").</summary>
    string? Version => null;

    /// <summary>Optional human-readable description of what this driver does.</summary>
    string? Description => null;

    /// <summary>
    /// Initialize the driver with configuration values.
    /// Open connections, start background tasks, etc.
    /// Config values are always native C# types — use DriverConfig helpers for safe extraction.
    /// </summary>
    Task InitAsync(Dictionary<string, object?>? config);

    /// <summary>Release all resources, close connections.</summary>
    Task ShutdownAsync();

    /// <summary>Declare all readable/writable properties.</summary>
    IEnumerable<PropertyDescriptor> GetProperties();

    /// <summary>Declare all executable actions.</summary>
    IEnumerable<ActionDescriptor> GetActions();

    /// <summary>Read a property value. Property name matches one returned by GetProperties().</summary>
    Task<object?> GetAsync(string property);

    /// <summary>
    /// Write a property value. Value is always a native C# type (int, double, string, bool, etc.).
    /// </summary>
    Task SetAsync(string property, object? value);

    /// <summary>
    /// Execute an action. Args can be null, a scalar, an array (object[]),
    /// or a dictionary (Dictionary&lt;string, object?&gt;).
    /// </summary>
    Task<object?> ExecuteAsync(string action, object? args);

    /// <summary>Functional group for UI categorization. Defaults to Instruments.</summary>
    DriverGroup Group => DriverGroup.Instruments;

    /// <summary>Whether this driver emits streaming data.</summary>
    bool SupportsStreaming => false;

    /// <summary>List of stream names this driver can emit.</summary>
    IEnumerable<string> GetStreams() => [];

    /// <summary>
    /// Set by the host to receive streaming data.
    /// Call as: StreamEmitter?.Invoke("streamName", "jsonData")
    /// </summary>
    Action<string, string>? StreamEmitter { get; set; }

    /// <summary>
    /// Scoped pub/sub handed in by the host. Null until the host assigns
    /// it (drivers that don't need it can ignore the property). See
    /// <see cref="IDriverHost"/> for the channel shape + scoping rules.
    /// </summary>
    IDriverHost? DriverHost { get => null; set { } }
}
