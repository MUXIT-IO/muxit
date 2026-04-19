// SPDX-License-Identifier: MIT
namespace Muxit.Driver.Sdk;

/// <summary>
/// Runtime support handed to a driver by its host. Provides scoped
/// pub/sub so drivers can subscribe to each other's streams (e.g. a
/// Vision driver subscribes to a Webcam connector's video stream).
///
/// Channel scoping
/// ---------------
/// All channel names must start with <c>"stream:"</c>. This confines
/// extensions to user-visible stream data and keeps internal server
/// events (log, license, diagnostics) private to MuxitServer.
///
/// Channel shape used across Muxit
/// -------------------------------
///   <c>stream:&lt;source-instance-id&gt;:&lt;stream-name&gt;</c>
/// e.g. <c>stream:webcam1:video</c>. Payloads are the native objects
/// the source driver emits via <see cref="IConnectorDriver.StreamEmitter"/>.
/// </summary>
public interface IDriverHost
{
    /// <summary>
    /// Subscribe to a stream channel. Returns a disposable that unsubscribes
    /// when disposed. Throws <see cref="ArgumentException"/> if the channel
    /// doesn't start with "stream:".
    /// </summary>
    IDisposable Subscribe(string channel, Action<object?> handler);

    /// <summary>
    /// Emit a value on a stream channel. Most drivers should prefer
    /// <see cref="IConnectorDriver.StreamEmitter"/>; this is here for
    /// drivers that need to fan data out on channels unrelated to their
    /// own name (e.g. a broker or bridge driver).
    /// </summary>
    void Emit(string channel, object? payload);
}
