// SPDX-License-Identifier: Apache-2.0
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

    /// <summary>
    /// Whether the safety gate (limits, confirmations, rate caps, audit log)
    /// should apply to this driver. Defaults to true. Set to false only for
    /// drivers with no path to physical hardware and no destructive actions
    /// (e.g. Webcam, FileAccess). For DLL drivers the assembly-level
    /// <see cref="RequiresSafetyGatesAttribute"/> overrides this — and only
    /// officially-signed DLLs are allowed to opt out.
    /// </summary>
    bool RequiresSafetyGates => true;

    /// <summary>List of stream names this driver can emit.</summary>
    IEnumerable<string> GetStreams() => [];

    /// <summary>
    /// Set by the host to receive streaming data.
    /// Call as: StreamEmitter?.Invoke("streamName", "jsonData")
    /// </summary>
    Action<string, string>? StreamEmitter { get; set; }

    /// <summary>
    /// Set by the host on drivers that produce audio. Hand the host a
    /// pre-rendered block of float PCM (interleaved if stereo, normalised
    /// to ±1.0); the host handles Opus encoding, JSON framing, real-time
    /// pacing, EventBus emission on the connector's <c>"audio"</c> stream,
    /// and the <c>{ "op": "stop" }</c> frame on cancellation.
    ///
    /// The returned <see cref="Task"/> completes when every sample has been
    /// streamed at real-time rate, or sooner if the cancellation token
    /// fires. <c>await</c> it before starting the next block to keep the
    /// driver in lockstep with the consumer's playback cursor.
    ///
    /// Drivers that emit audio should prefer this over the string
    /// <see cref="StreamEmitter"/> — it keeps codec dependencies out of
    /// driver assemblies and lets the host evolve the wire format without
    /// touching every audio driver.
    /// </summary>
    Func<float[], AudioFrameInfo, CancellationToken, Task>? AudioStreamEmitter { get => null; set { } }

    /// <summary>
    /// Set by the host on drivers that produce a *continuous* audio stream
    /// where samples are generated on demand (live mixing, polyphony, mic
    /// feeds, ...). The host pulls one chunk at a time from the supplied
    /// <see cref="IAsyncEnumerable{T}"/>, encodes each chunk through a
    /// single long-lived Opus encoder, and paces emission at real-time
    /// rate — same wire format and EventBus channel as
    /// <see cref="AudioStreamEmitter"/>, but without the per-call encoder
    /// restart that would otherwise glitch the decoder between voices.
    ///
    /// The driver may yield variable-length chunks; the host buffers and
    /// re-frames internally to Opus's fixed frame size. End the stream by
    /// completing the enumeration; cancellation sends the standard
    /// <c>{ "op": "stop" }</c> frame.
    /// </summary>
    Func<IAsyncEnumerable<float[]>, AudioFrameInfo, CancellationToken, Task>? AudioStreamReaderEmitter { get => null; set { } }

    /// <summary>
    /// Scoped pub/sub handed in by the host. Null until the host assigns
    /// it (drivers that don't need it can ignore the property). See
    /// <see cref="IDriverHost"/> for the channel shape + scoping rules.
    /// </summary>
    IDriverHost? DriverHost { get => null; set { } }
}
