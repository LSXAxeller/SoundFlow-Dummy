using System.Collections.Concurrent;
using SoundFlow.Metadata.Midi;
using SoundFlow.Metadata.Midi.Enums;
using SoundFlow.Midi.Devices;
using SoundFlow.Midi.Structs;
using SoundFlow.Providers;

namespace SoundFlow.Midi;

/// <summary>
/// A delegate that defines the contract for converting a real-time TimeSpan into a MIDI tick value.
/// </summary>
/// <param name="time">The time to convert.</param>
/// <returns>The corresponding value in MIDI ticks.</returns>
public delegate long TimeToTickConverter(TimeSpan time);

/// <summary>
/// A non-audio component that captures and timestamps MIDI messages with sample-level accuracy
/// relative to an audio engine's clock. This component is standalone and relies on its consumer
/// to provide the logic for converting time to MIDI ticks.
/// </summary>
public sealed class MidiRecorder : IDisposable
{
    private readonly record struct TimedMidiMessage(MidiMessage Message, long SampleTimestamp);

    private readonly MidiInputDevice _inputDevice;
    private readonly int _sampleRate;
    private readonly object _lock = new();

    private readonly ConcurrentQueue<TimedMidiMessage> _timedMessages = new();
    private long _totalSamplesProcessed;
    private long _currentLoopOffsetSamples;
    private bool _isRecording;

    /// <summary>
    /// Gets a value indicating whether the recorder is currently active.
    /// </summary>
    public bool IsRecording => _isRecording;
    
    /// <summary>
    /// Gets the timeline start time of the current recording session.
    /// </summary>
    public TimeSpan StartTime { get; private set; }

    /// <summary>
    /// Occurs when recording is stopped, providing the recorder instance and the resulting MidiDataProvider.
    /// </summary>
    public event Action<MidiRecorder, MidiDataProvider>? RecordingStopped;

    /// <summary>
    /// Initializes a new instance of the <see cref="MidiRecorder"/> class.
    /// </summary>
    /// <param name="inputDevice">The MIDI input device to record from.</param>
    /// <param name="sampleRate">The sample rate of the master clock (e.g., the composition's sample rate).</param>
    public MidiRecorder(MidiInputDevice inputDevice, int sampleRate)
    {
        if (sampleRate <= 0)
            throw new ArgumentOutOfRangeException(nameof(sampleRate), "Sample rate must be a positive number.");
        
        _inputDevice = inputDevice;
        _sampleRate = sampleRate;
    }

    /// <summary>
    /// Starts the recording process.
    /// </summary>
    public void StartRecording() => StartRecording(TimeSpan.Zero);
    
    internal void StartRecording(TimeSpan startTime)
    {
        lock (_lock)
        {
            if (_isRecording) return;
            StartTime = startTime;
            _totalSamplesProcessed = 0;
            _currentLoopOffsetSamples = 0;
            _timedMessages.Clear();
            _inputDevice.OnMessageReceived += OnMidiMessageReceived;
            _isRecording = true;
        }
    }

    /// <summary>
    /// Stops the recording process and generates a MidiDataProvider from the captured data.
    /// </summary>
    /// <param name="timeToTickConverter">A delegate function that converts a sample-accurate TimeSpan into a MIDI tick value based on the project's tempo map.</param>
    /// <param name="ticksPerQuarterNote">The time division to use for the resulting MIDI file.</param>
    public void StopRecording(TimeToTickConverter timeToTickConverter, int ticksPerQuarterNote = 480)
    {
        lock (_lock)
        {
            if (!_isRecording) return;
            _inputDevice.OnMessageReceived -= OnMidiMessageReceived;
            _isRecording = false;

            var provider = ProcessCapturedMessages(timeToTickConverter, ticksPerQuarterNote);
            RecordingStopped?.Invoke(this, provider);
        }
    }
    
    /// <summary>
    /// Adds a sample offset, used for loop recording to correctly place notes on subsequent passes.
    /// </summary>
    /// <param name="sampleOffset">The number of samples in the loop duration.</param>
    internal void AddLoopOffset(long sampleOffset)
    {
        lock (_lock)
        {
            _currentLoopOffsetSamples += sampleOffset;
        }
    }


    /// <summary>
    /// Updates the internal sample clock. This method must be called from a synchronized audio context,
    /// such as a master mixer's render loop, to ensure accurate timing.
    /// </summary>
    /// <param name="samplesInBlock">The number of samples processed in the last audio block.</param>
    public void UpdateSampleClock(int samplesInBlock)
    {
        if (!_isRecording) return;
        lock (_lock)
        {
            _totalSamplesProcessed += samplesInBlock;
        }
    }

    private void OnMidiMessageReceived(MidiMessage message, MidiDeviceInfo _)
    {
        if (!_isRecording) return;
        _timedMessages.Enqueue(new TimedMidiMessage(message, _totalSamplesProcessed + _currentLoopOffsetSamples));
    }

    private MidiDataProvider ProcessCapturedMessages(TimeToTickConverter timeToTickConverter, int ticksPerQuarterNote)
    {
        var messages = _timedMessages.OrderBy(m => m.SampleTimestamp).ToList();
        var midiFile = new MidiFile { Format = 1, TicksPerQuarterNote = ticksPerQuarterNote };
        var midiTrack = new MidiTrack();

        if (messages.Count > 0)
        {
            long lastEventTick = 0;
            foreach (var timedMessage in messages)
            {
                var eventTimeSpan = TimeSpan.FromSeconds((double)timedMessage.SampleTimestamp / _sampleRate);
                var absoluteTick = timeToTickConverter(eventTimeSpan);
                
                var deltaTicks = absoluteTick - lastEventTick;
                midiTrack.AddEvent(new ChannelEvent(deltaTicks, timedMessage.Message));
                lastEventTick = absoluteTick;
            }
        }
        
        midiTrack.AddEvent(new MetaEvent(0, MetaEventType.EndOfTrack, []));
        midiFile.AddTrack(midiTrack);

        return new MidiDataProvider(midiFile);
    }

    /// <inheritdoc />
    public void Dispose()
    {
        // If the recorder is disposed while still recording, it stops listening for messages
        // but does not finalize the captured data. The owner is responsible for calling StopRecording()
        // to retrieve the data before disposing.
        if (_isRecording)
        {
            _inputDevice.OnMessageReceived -= OnMidiMessageReceived;
            _isRecording = false;
        }
    }
}