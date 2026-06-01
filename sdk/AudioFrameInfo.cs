// SPDX-License-Identifier: Apache-2.0
namespace Muxit.Driver.Sdk;

/// <summary>
/// Format metadata for a block of float PCM audio handed to the host's
/// <see cref="IConnectorDriver.AudioStreamEmitter"/>. The host owns the
/// wire format (codec, framing, real-time pacing); drivers stay free of
/// audio-encoding concerns.
///
/// Samples must be normalised to ±1.0 and interleaved if
/// <see cref="Channels"/> &gt; 1. The host's encoder accepts any standard
/// Opus input rate (8 / 12 / 16 / 24 / 48 kHz); 48 kHz mono is the
/// preferred shape and avoids resampling on the encode path.
/// </summary>
public readonly record struct AudioFrameInfo(int SampleRate, int Channels)
{
    /// <summary>48 kHz mono — the host encoder's preferred input shape.</summary>
    public static AudioFrameInfo Mono48k => new(48000, 1);
}
