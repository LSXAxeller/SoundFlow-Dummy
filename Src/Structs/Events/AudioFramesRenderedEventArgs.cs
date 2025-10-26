using SoundFlow.Abstracts.Devices;

namespace SoundFlow.Structs.Events;

/// <summary>
/// Provides data for the AudioFramesRendered event.
/// </summary>
public class AudioFramesRenderedEventArgs(AudioDevice device, int frameCount) : EventArgs
{
    /// <summary>
    /// Gets the audio device that rendered the frames.
    /// </summary>
    public AudioDevice Device { get; } = device;

    /// <summary>
    /// Gets the number of audio frames that were rendered.
    /// </summary>
    public int FrameCount { get; } = frameCount;
}