using SoundFlow.Abstracts;
using SoundFlow.Enums;
using System.Buffers;
using System.Runtime.InteropServices;
using SoundFlow.Extensions.WebRtc.Apm.Abstracts;

namespace SoundFlow.Extensions.WebRtc.Apm.Modifiers;

/// <summary>
/// Applies WebRTC Acoustic Echo Cancellation (AEC) to an audio stream.
/// This modifier processes the near-end (microphone) audio stream passing through it,
/// while also listening to the global playback audio (far-end) via AudioEngine events
/// to provide the necessary reference signal to the AEC algorithm.
/// </summary>
public class EchoCancellationModifier : WebRtcModifierBase
{
    private bool _currentEnabledAec;
    private bool _currentMobileMode;
    private readonly int _aecLatencyMs;

    // Buffer for deinterleaving the far-end (playback) signal
    private float[][]? _deinterleavedFarendApmFrame;
    private readonly Queue<float> _farendInputRingBuffer = new();

    private IntPtr[]? _farendChannelPtrs;
    private IntPtr _farendChannelArrayPtr = IntPtr.Zero;
    private GCHandle _farendChannelArrayHandle;
    private IntPtr[]? _dummyReverseOutputChannelPtrs;
    private IntPtr _dummyReverseOutputChannelArrayPtr = IntPtr.Zero;
    private GCHandle _dummyReverseOutputChannelArrayHandle;

    public override string Name { get; set; } = "WebRTC Echo Cancellation";

    /// <summary>
    /// Gets or sets whether WebRTC Acoustic Echo Cancellation is enabled.
    /// </summary>
    public bool AecEnabled
    {
        get => _currentEnabledAec;
        set
        {
            if (_currentEnabledAec == value) return;
            _currentEnabledAec = value;
            ApplyAecConfig();
        }
    }

    /// <summary>
    /// Gets or sets whether to use the mobile mode for AEC.
    /// Mobile mode is generally more aggressive and optimized for mobile devices.
    /// </summary>
    public bool MobileMode
    {
        get => _currentMobileMode;
        set
        {
            if (_currentMobileMode == value) return;
            _currentMobileMode = value;
            ApplyAecConfig();
        }
    }

    /// <summary>
    /// Creates a new WebRTC Acoustic Echo Cancellation modifier.
    /// The modifier will be disabled if the current SoundFlow AudioEngine sample rate
    /// or channel count is not supported by WebRTC APM.
    /// </summary>
    /// <param name="initiallyEnabled">Initial enabled state for AEC.</param>
    /// <param name="mobileMode">Initial state for mobile mode.</param>
    /// <param name="aecLatencyMs">Estimated latency between playback and capture in milliseconds. Adjust based on system. Defaults to 40ms.</param>
    public EchoCancellationModifier(
        bool initiallyEnabled = true,
        bool mobileMode = false,
        int aecLatencyMs = 40)
    {
        _currentEnabledAec = initiallyEnabled;
        _currentMobileMode = mobileMode;
        _aecLatencyMs = aecLatencyMs;

        if (!Enabled || !IsApmSuccessfullyInitialized)
        {
            Enabled = false;
            return;
        }

        // Allocate AEC-specific managed and unmanaged buffers
        try
        {
            _deinterleavedFarendApmFrame = new float[NumChannels][];
            _farendChannelPtrs = new IntPtr[NumChannels];
            _dummyReverseOutputChannelPtrs = new IntPtr[NumChannels];

            for (var i = 0; i < NumChannels; i++)
            {
                _deinterleavedFarendApmFrame[i] = new float[ApmFrameSizePerChannel];
                _farendChannelPtrs[i] = Marshal.AllocHGlobal(ApmFrameSizeBytesPerChannel);
                _dummyReverseOutputChannelPtrs[i] = Marshal.AllocHGlobal(ApmFrameSizeBytesPerChannel);
            }

            _farendChannelArrayHandle = GCHandle.Alloc(_farendChannelPtrs, GCHandleType.Pinned);
            _farendChannelArrayPtr = _farendChannelArrayHandle.AddrOfPinnedObject();
            _dummyReverseOutputChannelArrayHandle = GCHandle.Alloc(_dummyReverseOutputChannelPtrs, GCHandleType.Pinned);
            _dummyReverseOutputChannelArrayPtr = _dummyReverseOutputChannelArrayHandle.AddrOfPinnedObject();

            AudioEngine.OnAudioProcessed += HandleAudioEngineProcessed;
            TrySetApmLatency();
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"WebRTC AEC Modifier: Farend buffer allocation failed: {ex.Message}. Disabling.");
            Enabled = false;
            DisposeFarendBuffers();
        }
    }

    private void ApplyAecConfig()
    {
        if (IsApmSuccessfullyInitialized && ApmConfig != null)
        {
            ApmConfig.SetEchoCanceller(_currentEnabledAec, _currentMobileMode);
            ReapplyApmConfiguration();
        }
    }

    /// <summary>
    /// Attempts to set the stream delay (latency) in the APM.
    /// Called after successful initialization.
    /// </summary>
    private void TrySetApmLatency()
    {
        if (!IsApmSuccessfullyInitialized || Apm == null) return;
        lock (ApmLock)
        {
            if (IsApmSuccessfullyInitialized && Apm != null) Apm.SetStreamDelayMs(_aecLatencyMs);
        }
    }

    protected override void ConfigureApmFeatures(ApmConfig config)
    {
        config.SetEchoCanceller(_currentEnabledAec, _currentMobileMode);
    }

    /// <summary>
    /// Handles the global AudioEngine processed event, specifically capturing Playback audio
    /// to feed the farend signal to the AEC.
    /// </summary>
    private void HandleAudioEngineProcessed(Span<float> samples, Capability capability)
    {
        if (capability != Capability.Playback || !Enabled || !IsApmSuccessfullyInitialized || Apm == null ||
            _deinterleavedFarendApmFrame == null || ReverseInputStreamConfig == null ||
            ReverseOutputStreamConfig == null || _farendChannelArrayPtr == IntPtr.Zero ||
            _dummyReverseOutputChannelArrayPtr == IntPtr.Zero || samples.Length == 0)
            return;

        foreach (var sample in samples) _farendInputRingBuffer.Enqueue(sample);

        var totalSamplesInApmFrame = ApmFrameSizePerChannel * NumChannels;

        while (_farendInputRingBuffer.Count >= totalSamplesInApmFrame)
        {
            var currentApmInterleavedFarendFrame = ArrayPool<float>.Shared.Rent(totalSamplesInApmFrame);
            try
            {
                for (var i = 0; i < totalSamplesInApmFrame; i++)
                    if (!_farendInputRingBuffer.TryDequeue(out currentApmInterleavedFarendFrame[i]))
                        break;

                // Deinterleave Farend frame into its dedicated buffer
                Deinterleave(currentApmInterleavedFarendFrame.AsSpan(0, totalSamplesInApmFrame),
                    NumChannels, ApmFrameSizePerChannel, _deinterleavedFarendApmFrame);

                for (var ch = 0; ch < NumChannels; ch++)
                    Marshal.Copy(_deinterleavedFarendApmFrame[ch], 0, _farendChannelPtrs![ch], ApmFrameSizePerChannel);

                ApmError error;
                lock (ApmLock)
                {
                    if (!IsApmSuccessfullyInitialized || Apm == null) error = ApmError.UnspecifiedError;
                    else
                    {
                        error = NativeMethods.webrtc_apm_process_reverse_stream(
                            Apm.NativePtr,
                            _farendChannelArrayPtr,
                            ReverseInputStreamConfig.NativePtr,
                            ReverseOutputStreamConfig.NativePtr,
                            _dummyReverseOutputChannelArrayPtr);
                    }
                }

                if (error != ApmError.NoError)
                    Console.Error.WriteLine($"WebRTC AEC: Error processing reverse stream: {error}.");
            }
            finally
            {
                ArrayPool<float>.Shared.Return(currentApmInterleavedFarendFrame);
            }
        }
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            if (IsApmSuccessfullyInitialized)
                AudioEngine.OnAudioProcessed -= HandleAudioEngineProcessed;
            DisposeFarendBuffers();
            _farendInputRingBuffer.Clear();
            _deinterleavedFarendApmFrame = null;
        }

        base.Dispose(disposing);
    }

    private void DisposeFarendBuffers()
    {
        lock (ApmLock)
        {
            if (_farendChannelArrayHandle.IsAllocated) _farendChannelArrayHandle.Free();
            if (_dummyReverseOutputChannelArrayHandle.IsAllocated) _dummyReverseOutputChannelArrayHandle.Free();
            _farendChannelArrayPtr = IntPtr.Zero;
            _dummyReverseOutputChannelArrayPtr = IntPtr.Zero;

            if (_farendChannelPtrs != null)
            {
                foreach (var ptr in _farendChannelPtrs)
                    if (ptr != IntPtr.Zero)
                        Marshal.FreeHGlobal(ptr);
                _farendChannelPtrs = null;
            }

            if (_dummyReverseOutputChannelPtrs != null)
            {
                foreach (var ptr in _dummyReverseOutputChannelPtrs)
                    if (ptr != IntPtr.Zero)
                        Marshal.FreeHGlobal(ptr);
                _dummyReverseOutputChannelPtrs = null;
            }
        }
    }
}