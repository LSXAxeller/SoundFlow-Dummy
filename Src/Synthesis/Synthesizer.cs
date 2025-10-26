using System.Buffers;
using SoundFlow.Abstracts;
using SoundFlow.Midi.Enums;
using SoundFlow.Midi.Interfaces;
using SoundFlow.Midi.Routing;
using SoundFlow.Midi.Structs;
using SoundFlow.Structs;
using SoundFlow.Synthesis.Interfaces;
using SoundFlow.Synthesis.Voices;

namespace SoundFlow.Synthesis;

/// <summary>
/// A polyphonic, multi-timbral synthesizer component that generates audio from MIDI messages.
/// </summary>
public sealed class Synthesizer : SoundComponent, IMidiControllable
{
    private readonly MidiChannel[] _channels = new MidiChannel[16];
    private readonly Dictionary<int, IVoice> _mpeNoteToVoiceMap = new(); // Note number -> Active MPE voice
    private bool _mpeEnabled;

    /// <inheritdoc />
    public override string Name { get; set; } = "Synthesizer";
    
    /// <summary>
    /// Gets or sets whether the synthesizer operates in MPE (MIDI Polyphonic Expression) mode.
    /// </summary>
    public bool MpeEnabled
    {
        get => _mpeEnabled;
        set
        {
            if (_mpeEnabled == value) return;
            _mpeEnabled = value;
            
            // When switching modes, send an "All Notes Off" to prevent stuck notes.
            ProcessMidiMessage(new MidiMessage(0xB0, 123, 0));
        }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Synthesizer"/> class.
    /// </summary>
    /// <param name="engine">The parent audio engine.</param>
    /// <param name="format">The audio format for the synthesizer's output.</param>
    /// <param name="instrumentBank">The bank of instruments the synthesizer will use.</param>
    public Synthesizer(AudioEngine engine, AudioFormat format, IInstrumentBank instrumentBank) : base(engine, format)
    {
        for (var i = 0; i < _channels.Length; i++)
        {
            _channels[i] = new MidiChannel(format, instrumentBank);
        }
    }

    /// <inheritdoc />
    public void ProcessMidiMessage(MidiMessage message)
    {
        var channelIndex = message.Channel - 1;
        if (channelIndex < 0 || channelIndex >= _channels.Length) return;

        if (MpeEnabled)
        {
            switch (message.Command)
            {
                case MidiCommand.NoteOn when message.Velocity > 0:
                    var voice = _channels[channelIndex].NoteOn(message.NoteNumber, message.Velocity);
                    _mpeNoteToVoiceMap[message.NoteNumber] = voice;
                    break;
                case MidiCommand.NoteOff:
                case MidiCommand.NoteOn when message.Velocity == 0:
                    _channels[channelIndex].NoteOff(message.NoteNumber);
                    _mpeNoteToVoiceMap.Remove(message.NoteNumber);
                    break;
                default:
                    // Global messages (like sustain pedal) on the master channel are handled normally
                    _channels[channelIndex].ProcessMidiMessage(message);
                    break;
            }
        }
        else
        {
            // Standard multi-timbral behavior
            _channels[channelIndex].ProcessMidiMessage(message);
        }
    }

    /// <summary>
    /// For internal use by MidiManager. Processes a high-level MPE event.
    /// </summary>
    /// <param name="mpeEvent">The MPE event object.</param>
    internal void ProcessMpeEvent(object mpeEvent)
    {
        if (!MpeEnabled) return;

        switch (mpeEvent)
        {
            case MidiMessage msg:
                // This will be a Note On/Off from the MPE parser
                ProcessMidiMessage(msg);
                break;
            case MidiManager.GlobalPitchBendEvent gpb:
                // Global pitch bend, typically +/- 2 semitones.
                var bendSemitones = (gpb.PitchBendValue - 8192) / 8192.0f * 2.0f;
                foreach (var voice in _mpeNoteToVoiceMap.Values)
                {
                    // The 'channelPitchBend' parameter is used for global/channel-wide bend.
                    voice.ProcessMidiControl(default, bendSemitones);
                }
                break;
            case MidiManager.PerNotePitchBendEvent pb:
                if (_mpeNoteToVoiceMap.TryGetValue(pb.NoteNumber, out var pbVoice)) pbVoice.SetPerNotePitchBend(pb.BendSemitones);
                
                break;
            case MidiManager.PerNotePressureEvent p:
                if (_mpeNoteToVoiceMap.TryGetValue(p.NoteNumber, out var pVoice)) 
                    pVoice.SetPerNotePressure(p.Pressure);
                break;
            case MidiManager.PerNoteTimbreEvent t:
                if (_mpeNoteToVoiceMap.TryGetValue(t.NoteNumber, out var tVoice)) 
                    tVoice.SetPerNoteTimbre(t.Timbre);
                break;
        }
    }

    /// <inheritdoc />
    protected override void GenerateAudio(Span<float> buffer, int channels)
    {
        buffer.Clear();
        float[]? rentedBuffer = null;

        try
        {
            rentedBuffer = ArrayPool<float>.Shared.Rent(buffer.Length);
            var tempBuffer = rentedBuffer.AsSpan(0, buffer.Length);

            foreach (var channel in _channels)
            {
                channel.Render(tempBuffer);

                // Mix the channel's output into the main buffer
                for (var i = 0; i < buffer.Length; i++)
                {
                    buffer[i] += tempBuffer[i];
                }
            }
        }
        finally
        {
            if (rentedBuffer != null) 
                ArrayPool<float>.Shared.Return(rentedBuffer);
        }
    }
}