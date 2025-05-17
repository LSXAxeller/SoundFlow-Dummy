using System.Buffers;
using System.Runtime.InteropServices;
using SoundFlow.Abstracts;

namespace SoundFlow.Extensions.WebRtc.Apm.Abstracts;

public abstract class WebRtcModifierBase : SoundModifier, IDisposable
{
    protected AudioProcessingModule? Apm { get; private set; }
    protected ApmConfig? ApmConfig { get; private set; }
    protected readonly object ApmLock = new();

    protected StreamConfig? InputStreamConfig;
    protected StreamConfig? OutputStreamConfig;
    protected StreamConfig? ReverseInputStreamConfig;
    protected StreamConfig? ReverseOutputStreamConfig;

    protected readonly int ApmFrameSizePerChannel; // Samples per channel for a 10ms APM frame
    protected readonly int NumChannels;
    protected readonly int SampleRate;
    protected const int BytesPerSample = sizeof(float);
    protected readonly int ApmFrameSizeBytesPerChannel;

    protected float[][]? DeinterleavedInputApmFrame;
    protected float[][]? DeinterleavedOutputApmFrame;
    
    public float PostProcessGain { get; set; } = 1f;

    private IntPtr[]? _inputChannelPtrs;
    private IntPtr[]? _outputChannelPtrs;
    private IntPtr _inputChannelArrayPtr = IntPtr.Zero;
    private IntPtr _outputChannelArrayPtr = IntPtr.Zero;
    private GCHandle _inputChannelArrayHandle;
    private GCHandle _outputChannelArrayHandle;

    private readonly Queue<float> _inputRingBuffer = [];
    private readonly Queue<float> _outputRingBuffer = [];

    protected bool IsApmSuccessfullyInitialized { get; private set; }
    private bool _isDisposed;

    protected WebRtcModifierBase()
    {
        SampleRate = AudioEngine.Instance.SampleRate;
        NumChannels = AudioEngine.Channels;

        if (SampleRate != 8000 && SampleRate != 16000 && SampleRate != 32000 && SampleRate != 48000)
            throw new ArgumentException($"Unsupported sample rate for WebRTC Audio Processing Module: {SampleRate} Hz. Must be 8k, 16k, 32k, or 48k.");

        ApmFrameSizePerChannel = AudioProcessingModule.GetFrameSize(SampleRate);
        ApmFrameSizeBytesPerChannel = ApmFrameSizePerChannel * BytesPerSample;

        if (ApmFrameSizePerChannel == 0)
        {
            Console.Error.WriteLine($"WebRTC APM Modifier ({GetType().Name}): Unsupported sample rate: {SampleRate} Hz. Disabling.");
            Enabled = false;
            return;
        }
        if (NumChannels <= 0)
        {
            Console.Error.WriteLine($"WebRTC APM Modifier ({GetType().Name}): Invalid channel count: {NumChannels}. Disabling.");
            Enabled = false;
            return;
        }

        InitializeApmAndFeatures();
    }

    private void InitializeApmAndFeatures()
    {
        if (IsApmSuccessfullyInitialized) return;

        lock (ApmLock)
        {
            if (IsApmSuccessfullyInitialized) return;
            try
            {
                Apm = new AudioProcessingModule();
                ApmConfig = new ApmConfig();

                // Let derived class configure its specific features
                ConfigureApmFeatures(ApmConfig);

                var applyError = Apm.ApplyConfig(ApmConfig);
                if (applyError != ApmError.NoError)
                    throw new InvalidOperationException($"Failed to apply APM config: {applyError}");

                // Create stream configs
                InputStreamConfig = new StreamConfig(SampleRate, NumChannels);
                OutputStreamConfig = new StreamConfig(SampleRate, NumChannels);
                ReverseInputStreamConfig = new StreamConfig(SampleRate, NumChannels);
                ReverseOutputStreamConfig = new StreamConfig(SampleRate, NumChannels);


                // Initialize APM
                var initError = Apm.Initialize();
                if (initError != ApmError.NoError)
                    throw new InvalidOperationException($"Failed to initialize APM: {initError}");

                DeinterleavedInputApmFrame = new float[NumChannels][];
                DeinterleavedOutputApmFrame = new float[NumChannels][];
                _inputChannelPtrs = new IntPtr[NumChannels];
                _outputChannelPtrs = new IntPtr[NumChannels];

                for (var i = 0; i < NumChannels; i++)
                {
                    DeinterleavedInputApmFrame[i] = new float[ApmFrameSizePerChannel];
                    DeinterleavedOutputApmFrame[i] = new float[ApmFrameSizePerChannel];
                    _inputChannelPtrs[i] = Marshal.AllocHGlobal(ApmFrameSizeBytesPerChannel);
                    _outputChannelPtrs[i] = Marshal.AllocHGlobal(ApmFrameSizeBytesPerChannel);
                }

                _inputChannelArrayHandle = GCHandle.Alloc(_inputChannelPtrs, GCHandleType.Pinned);
                _inputChannelArrayPtr = _inputChannelArrayHandle.AddrOfPinnedObject();
                _outputChannelArrayHandle = GCHandle.Alloc(_outputChannelPtrs, GCHandleType.Pinned);
                _outputChannelArrayPtr = _outputChannelArrayHandle.AddrOfPinnedObject();

                IsApmSuccessfullyInitialized = true;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"WebRTC APM Modifier ({GetType().Name}): Initialization Exception: {ex.Message}");
                Enabled = false;
                DisposeApmNativeResources();
                throw;
            }
        }
    }

    protected abstract void ConfigureApmFeatures(ApmConfig config);

    protected void ReapplyApmConfiguration()
    {
        if (!Enabled || !IsApmSuccessfullyInitialized || Apm == null || ApmConfig == null) return;
        lock (ApmLock)
        {
            if (!IsApmSuccessfullyInitialized) return;
            var error = Apm.ApplyConfig(ApmConfig);
            if (error != ApmError.NoError)
                Console.Error.WriteLine($"WebRTC APM Modifier {Name}: Failed to re-apply APM config: {error}.");
        }
    }

    public override void Process(Span<float> buffer)
    {
        if (!Enabled || !IsApmSuccessfullyInitialized || Apm == null || ApmConfig == null ||
            InputStreamConfig == null || OutputStreamConfig == null || DeinterleavedInputApmFrame == null ||
            DeinterleavedOutputApmFrame == null || _inputChannelArrayPtr == IntPtr.Zero ||
            _outputChannelArrayPtr == IntPtr.Zero || buffer.Length == 0) return;

        var samplesToProcess = buffer.Length;
        for (var i = 0; i < samplesToProcess; i++) _inputRingBuffer.Enqueue(buffer[i]);

        var totalSamplesInApmFrame = ApmFrameSizePerChannel * NumChannels;
        var processedFrames = false;

        while (_inputRingBuffer.Count >= totalSamplesInApmFrame)
        {
            processedFrames = true;
            var currentApmInterleavedInputFrame = ArrayPool<float>.Shared.Rent(totalSamplesInApmFrame);
            try
            {
                for (var i = 0; i < totalSamplesInApmFrame; i++)
                    if (!_inputRingBuffer.TryDequeue(out currentApmInterleavedInputFrame[i])) break; // Something went wrong

                Deinterleave(currentApmInterleavedInputFrame.AsSpan(0, totalSamplesInApmFrame),
                    NumChannels, ApmFrameSizePerChannel, DeinterleavedInputApmFrame);

                for (var ch = 0; ch < NumChannels; ch++)
                    Marshal.Copy(DeinterleavedInputApmFrame[ch], 0, _inputChannelPtrs![ch], ApmFrameSizePerChannel);

                ApmError error;
                lock (ApmLock)
                {
                    if (!IsApmSuccessfullyInitialized || Apm == null) error = ApmError.UnspecifiedError;
                    else
                    {
                        error = NativeMethods.webrtc_apm_process_stream(
                            Apm.NativePtr,
                            _inputChannelArrayPtr,
                            InputStreamConfig.NativePtr,
                            OutputStreamConfig.NativePtr,
                            _outputChannelArrayPtr);
                    }
                }

                var resultBufferToInterleave = DeinterleavedInputApmFrame; // Default to pass-through on error

                if (error == ApmError.NoError)
                {
                    for (var ch = 0; ch < NumChannels; ch++)
                        Marshal.Copy(_outputChannelPtrs![ch], DeinterleavedOutputApmFrame[ch], 0, ApmFrameSizePerChannel);
                    resultBufferToInterleave = DeinterleavedOutputApmFrame;
                }
                else
                {
                     Console.Error.WriteLine($"WebRTC APM {Name}: Error processing stream: {error}. Passing through frame.");
                }

                // Interleave the result (processed or original) into temp buffer
                Interleave(resultBufferToInterleave, NumChannels, ApmFrameSizePerChannel,
                    currentApmInterleavedInputFrame.AsSpan(0, totalSamplesInApmFrame));

                for (var i = 0; i < totalSamplesInApmFrame; i++)
                    _outputRingBuffer.Enqueue(currentApmInterleavedInputFrame[i]);
            }
            finally
            {
                ArrayPool<float>.Shared.Return(currentApmInterleavedInputFrame);
            }
        }

        for (var i = 0; i < samplesToProcess; i++)
        {
            if (_outputRingBuffer.TryDequeue(out var sample)) buffer[i] = sample * PostProcessGain;
            else
            {
                if (processedFrames) buffer[i..].Clear(); // Underrun, fill rest with silence
                break;
            }
        }
    }

    public override float ProcessSample(float sample, int channel) => throw new NotSupportedException();

    protected static void Deinterleave(ReadOnlySpan<float> interleaved, int numChannels, int frameSizePerChannel, float[][]? deinterleavedTarget)
    {
        if (deinterleavedTarget == null) return;
        for (var ch = 0; ch < numChannels; ch++)
        {
            if (deinterleavedTarget[ch] == null || deinterleavedTarget[ch].Length != frameSizePerChannel)
                deinterleavedTarget[ch] = new float[frameSizePerChannel];
            for (var i = 0; i < frameSizePerChannel; i++)
            {
                var idx = i * numChannels + ch;
                deinterleavedTarget[ch][i] = idx < interleaved.Length ? interleaved[idx] : 0f;
            }
        }
    }

    protected static void Interleave(float[][] deinterleaved, int numChannels, int frameSizePerChannel, Span<float> interleavedTarget)
    {
        for (var ch = 0; ch < numChannels; ch++)
        {
            if (deinterleaved?[ch] == null || deinterleaved[ch].Length < frameSizePerChannel) continue;
            for (var i = 0; i < frameSizePerChannel; i++)
            {
                var idx = i * numChannels + ch;
                if (idx < interleavedTarget.Length) interleavedTarget[idx] = deinterleaved[ch][i];
            }
        }
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_isDisposed)
        {
            if (disposing)
            {
                DisposeApmNativeResources();
                _inputRingBuffer.Clear();
                _outputRingBuffer.Clear();
            }
            _isDisposed = true;
        }
    }

    private void DisposeApmNativeResources()
    {
        lock (ApmLock)
        {
            if (_inputChannelArrayHandle.IsAllocated) _inputChannelArrayHandle.Free();
            if (_outputChannelArrayHandle.IsAllocated) _outputChannelArrayHandle.Free();
            _inputChannelArrayPtr = IntPtr.Zero;
            _outputChannelArrayPtr = IntPtr.Zero;

            if (_inputChannelPtrs != null)
            {
                foreach (var ptr in _inputChannelPtrs)
                    if (ptr != IntPtr.Zero) Marshal.FreeHGlobal(ptr);
                _inputChannelPtrs = null;
            }
            if (_outputChannelPtrs != null)
            {
                foreach (var ptr in _outputChannelPtrs)
                    if (ptr != IntPtr.Zero) Marshal.FreeHGlobal(ptr);
                _outputChannelPtrs = null;
            }

            Apm?.Dispose();
            Apm = null;
            ApmConfig?.Dispose(); ApmConfig = null;
            InputStreamConfig?.Dispose(); InputStreamConfig = null;
            OutputStreamConfig?.Dispose(); OutputStreamConfig = null;
            ReverseInputStreamConfig?.Dispose(); ReverseInputStreamConfig = null;
            ReverseOutputStreamConfig?.Dispose(); ReverseOutputStreamConfig = null;

            IsApmSuccessfullyInitialized = false;
        }
    }

    ~WebRtcModifierBase()
    {
        Dispose(false);
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}