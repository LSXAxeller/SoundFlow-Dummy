using SoundFlow.Interfaces;
using SoundFlow.Metadata.Midi;
using SoundFlow.Midi.Interfaces;
using SoundFlow.Midi.Routing.Nodes;
using SoundFlow.Midi.Structs;
using SoundFlow.Structs;
using SoundFlow.Utils;

namespace SoundFlow.Editing;

/// <summary>
/// Represents a MIDI track within a composition, containing a collection of MIDI segments
/// and routing their output to a specific MIDI-controllable target.
/// </summary>
public class MidiTrack
{
    private string _name;
    private IMidiDestinationNode? _target;
    private TrackSettings _settings;
    private Composition? _parentComposition;

    /// <summary>
    /// Gets or sets the name of the track.
    /// </summary>
    public string Name
    {
        get => _name;
        set
        {
            if (_name == value) return;
            _name = value;
            MarkDirty();
        }
    }

    /// <summary>
    /// Gets the list of <see cref="MidiSegment"/>s contained within this track.
    /// </summary>
    public List<MidiSegment> Segments { get; } = [];

    /// <summary>
    /// Gets or sets the target node (e.g., a Synthesizer or a physical MIDI output) that will receive MIDI events from this track.
    /// </summary>
    public IMidiDestinationNode? Target
    {
        get => _target;
        set
        {
            if (_target == value) return;
            _target = value;
            MarkDirty();
        }
    }

    /// <summary>
    /// Gets or sets the settings applied to this track. While not all settings (like Volume/Pan) directly
    /// apply to MIDI data, Mute/Solo/IsEnabled and MIDI modifiers are relevant.
    /// </summary>
    public TrackSettings Settings
    {
        get => _settings;
        set
        {
            if (_settings == value) return;
            _settings = value ?? throw new ArgumentNullException(nameof(value));
            _settings.ParentTrack = null; // MIDI tracks don't have the same parentage concept for audio
            MarkDirty();
        }
    }

    /// <summary>
    /// Gets or sets the parent composition to which this track belongs.
    /// </summary>
    public Composition? ParentComposition
    {
        get => _parentComposition;
        set => _parentComposition = value;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MidiTrack"/> class.
    /// </summary>
    /// <param name="name">The name of the track.</param>
    /// <param name="target">The initial target for MIDI events.</param>
    /// <param name="settings">Optional track settings.</param>
    public MidiTrack(string name = "MIDI Track", IMidiDestinationNode? target = null, TrackSettings? settings = null)
    {
        _name = name;
        _target = target;
        _settings = settings ?? new TrackSettings();
    }

    /// <summary>
    /// Renders the MIDI output for this track for a given time range by sending MIDI events to its target.
    /// This method does not produce audio directly.
    /// </summary>
    /// <param name="startTime">The global timeline start time for rendering.</param>
    /// <param name="endTime">The global timeline end time for rendering.</param>
    public void Render(TimeSpan startTime, TimeSpan endTime)
    {
        if (!Settings.IsEnabled || Settings.IsMuted || Target == null || ParentComposition == null)
        {
            return;
        }

        foreach (var segment in Segments)
        {
            var segmentStart = segment.TimelineStartTime;
            var segmentEnd = segmentStart + segment.SourceDuration;

            // Check for overlap between the render window and the segment
            if (startTime < segmentEnd && endTime > segmentStart)
            {
                var overlapStart = startTime > segmentStart ? startTime : segmentStart;
                var overlapEnd = endTime < segmentEnd ? endTime : segmentEnd;
                
                var timeIntoSegmentStart = overlapStart - segmentStart;
                var timeIntoSegmentEnd = overlapEnd - segmentStart;

                var startTick = MidiTimeConverter.GetTickForTimeSpan(timeIntoSegmentStart, segment.DataProvider.TicksPerQuarterNote, ParentComposition.TempoTrack);
                var endTick = MidiTimeConverter.GetTickForTimeSpan(timeIntoSegmentEnd, segment.DataProvider.TicksPerQuarterNote, ParentComposition.TempoTrack);

                foreach (var timedEvent in segment.DataProvider.GetEvents(startTick, endTick))
                {
                    switch (timedEvent.Event)
                    {
                        case ChannelEvent channelEvent:
                        {
                            var messagesToProcess = new List<MidiMessage> { channelEvent.Message };
                            foreach (var modifier in Settings.MidiModifiers)
                            {
                                if (!modifier.IsEnabled) continue;
                            
                                var nextMessages = new List<MidiMessage>();
                                foreach (var msg in messagesToProcess)
                                {
                                    nextMessages.AddRange(modifier.Process(msg));
                                }
                                messagesToProcess = nextMessages;
                            }

                            foreach (var finalMessage in messagesToProcess)
                            {
                                Target.ProcessMessage(finalMessage);
                            }

                            break;
                        }
                        case SysExEvent sysExEvent:
                        {
                            // SysEx messages bypass the modifier chain and are only sent to physical output devices.
                            if (Target is MidiOutputNode { Device.IsDisposed: false } outputNode) 
                                outputNode.Device.SendSysEx(sysExEvent.Data);

                            break;
                        }
                    }
                }
            }
        }
    }
    
    /// <summary>
    /// Adds a <see cref="MidiSegment"/> to the track and re-sorts the segments by time.
    /// </summary>
    /// <param name="segment">The MIDI segment to add.</param>
    public void AddSegment(MidiSegment segment)
    {
        segment.ParentTrack = this;
        Segments.Add(segment);
        Segments.Sort((a, b) => a.TimelineStartTime.CompareTo(b.TimelineStartTime));
        MarkDirty();
    }
    
    /// <summary>
    /// Removes a <see cref="MidiSegment"/> from the track.
    /// </summary>
    /// <param name="segment">The MIDI segment to remove.</param>
    /// <returns>True if the segment was successfully removed, false otherwise.</returns>
    public bool RemoveSegment(MidiSegment segment)
    {
        segment.ParentTrack = null;
        var removed = Segments.Remove(segment);
        if(removed) MarkDirty();
        return removed;
    }
    
    /// <summary>
    /// Calculates the total duration of the track based on the latest ending MIDI segment.
    /// </summary>
    /// <returns>A <see cref="TimeSpan"/> representing the total duration of the track.</returns>
    public TimeSpan CalculateDuration()
    {
        return Segments.Count == 0 ? TimeSpan.Zero : Segments.Max(s => s.TimelineEndTime);
    }

    /// <summary>
    /// Marks the parent composition as dirty (having unsaved changes).
    /// </summary>
    public void MarkDirty()
    {
        ParentComposition?.MarkDirty();
    }
}