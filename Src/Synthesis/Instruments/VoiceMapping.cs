namespace SoundFlow.Synthesis.Instruments;

/// <summary>
/// Defines a mapping that links a specific range of MIDI notes and velocities
/// to a particular VoiceDefinition. This is the core of a multi-sampled instrument.
/// </summary>
public class VoiceMapping
{
    /// <summary>
    /// The VoiceDefinition to use when the conditions of this mapping are met.
    /// </summary>
    public VoiceDefinition Definition { get; }

    /// <summary>
    /// The minimum MIDI note number for this mapping (inclusive).
    /// </summary>
    public int MinKey { get; set; }

    /// <summary>
    /// The maximum MIDI note number for this mapping (inclusive).
    /// </summary>
    public int MaxKey { get; set; } = 127;

    /// <summary>
    /// The minimum velocity for this mapping (inclusive).
    /// </summary>
    public int MinVelocity { get; set; }

    /// <summary>
    /// The maximum velocity for this mapping (inclusive).
    /// </summary>
    public int MaxVelocity { get; set; } = 127;
    
    // SF2 Specific Parameters
    public float InitialAttenuation { get; set; } // in dB
    public float Pan { get; set; } // -1 to 1
    public int RootKeyOverride { get; set; } = -1;
    public int Tune { get; set; } // in cents
    public int LoopMode { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="VoiceMapping"/> class.
    /// </summary>
    /// <param name="definition">The VoiceDefinition for this mapping.</param>
    public VoiceMapping(VoiceDefinition definition)
    {
        Definition = definition;
    }

    /// <summary>
    /// Checks if a given note number and velocity fall within this mapping's range.
    /// </summary>
    /// <param name="noteNumber">The MIDI note number to check.</param>
    /// <param name="velocity">The velocity to check.</param>
    /// <returns>True if the note and velocity are within the defined ranges.</returns>
    public bool IsMatch(int noteNumber, int velocity)
    {
        return noteNumber >= MinKey && noteNumber <= MaxKey &&
               velocity >= MinVelocity && velocity <= MaxVelocity;
    }
}