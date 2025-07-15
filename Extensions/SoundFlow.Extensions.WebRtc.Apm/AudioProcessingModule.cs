using System.Reflection;
using System.Runtime.InteropServices;

namespace SoundFlow.Extensions.WebRtc.Apm;

#pragma warning disable CS1591

/// <summary>
/// Error codes returned by the WebRTC Audio Processing Module
/// </summary>
public enum ApmError
{
    NoError = 0,
    UnspecifiedError = -1,
    CreationFailed = -2,
    BadParameter = -6,
    BadSampleRate = -7,
    BadDataLength = -8,
    BadNumChannels = -9
}

/// <summary>
/// Noise suppression levels
/// </summary>
public enum NoiseSuppressionLevel
{
    Low,
    Moderate,
    High,
    VeryHigh
}

/// <summary>
/// Gain controller modes
/// </summary>
public enum GainControlMode
{
    AdaptiveAnalog,
    AdaptiveDigital,
    FixedDigital
}

/// <summary>
/// Downmix methods
/// </summary>
public enum DownmixMethod
{
    AverageChannels,
    UseFirstChannel
}

/// <summary>
/// Runtime setting types
/// </summary>
public enum RuntimeSettingType
{
    CapturePreGain,
    CaptureCompressionGain,
    CaptureFixedPostGain,
    PlayoutVolumeChange,
    CustomRenderSetting,
    PlayoutAudioDeviceChange,
    CapturePostGain,
    CaptureOutputUsed
}
#pragma warning restore CS1591

/// <summary>
/// Represents a stream configuration for audio processing
/// </summary>
public class StreamConfig : IDisposable
{
    private IntPtr _nativeConfig;

    /// <summary>
    /// Creates a new stream configuration
    /// </summary>
    /// <param name="sampleRateHz">Sample rate in Hz</param>
    /// <param name="numChannels">Number of channels</param>
    public StreamConfig(int sampleRateHz, int numChannels)
    {
        _nativeConfig = NativeMethods.webrtc_apm_stream_config_create(sampleRateHz, (UIntPtr)numChannels);
        if (_nativeConfig == IntPtr.Zero)
            throw new InvalidOperationException("Failed to create stream config");
    }

    /// <summary>
    /// Sample rate in Hz
    /// </summary>
    public int SampleRateHz => NativeMethods.webrtc_apm_stream_config_sample_rate_hz(_nativeConfig);

    /// <summary>
    /// Number of channels
    /// </summary>
    public int NumChannels => (int)NativeMethods.webrtc_apm_stream_config_num_channels(_nativeConfig);

    internal IntPtr NativePtr => _nativeConfig;

    #region IDisposable Support

    private bool disposedValue = false;

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (_nativeConfig != IntPtr.Zero)
            {
                NativeMethods.webrtc_apm_stream_config_destroy(_nativeConfig);
                _nativeConfig = IntPtr.Zero;
            }

            disposedValue = true;
        }
    }

    ~StreamConfig()
    {
        Dispose(false);
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    #endregion
}

/// <summary>
/// Represents a processing configuration for audio processing
/// </summary>
public class ProcessingConfig : IDisposable
{
    private IntPtr _nativeConfig;

    /// <summary>
    /// Creates a new processing configuration
    /// </summary>
    public ProcessingConfig()
    {
        _nativeConfig = NativeMethods.webrtc_apm_processing_config_create();
        if (_nativeConfig == IntPtr.Zero)
            throw new InvalidOperationException("Failed to create processing config");
    }

    /// <summary>
    /// Input stream configuration
    /// </summary>
    public StreamConfig InputStream
    {
        get
        {
            var ptr = NativeMethods.webrtc_apm_processing_config_input_stream(_nativeConfig);
            return new StreamConfig(
                NativeMethods.webrtc_apm_stream_config_sample_rate_hz(ptr),
                (int)NativeMethods.webrtc_apm_stream_config_num_channels(ptr));
        }
    }

    /// <summary>
    /// Output stream configuration
    /// </summary>
    public StreamConfig OutputStream
    {
        get
        {
            var ptr = NativeMethods.webrtc_apm_processing_config_output_stream(_nativeConfig);
            return new StreamConfig(
                NativeMethods.webrtc_apm_stream_config_sample_rate_hz(ptr),
                (int)NativeMethods.webrtc_apm_stream_config_num_channels(ptr));
        }
    }

    /// <summary>
    /// Reverse input stream configuration
    /// </summary>
    public StreamConfig ReverseInputStream
    {
        get
        {
            var ptr = NativeMethods.webrtc_apm_processing_config_reverse_input_stream(_nativeConfig);
            return new StreamConfig(
                NativeMethods.webrtc_apm_stream_config_sample_rate_hz(ptr),
                (int)NativeMethods.webrtc_apm_stream_config_num_channels(ptr));
        }
    }

    /// <summary>
    /// Reverse output stream configuration
    /// </summary>
    public StreamConfig ReverseOutputStream
    {
        get
        {
            var ptr = NativeMethods.webrtc_apm_processing_config_reverse_output_stream(_nativeConfig);
            return new StreamConfig(
                NativeMethods.webrtc_apm_stream_config_sample_rate_hz(ptr),
                (int)NativeMethods.webrtc_apm_stream_config_num_channels(ptr));
        }
    }

    internal IntPtr NativePtr => _nativeConfig;

    #region IDisposable Support

    private bool disposedValue = false;

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (_nativeConfig != IntPtr.Zero)
            {
                NativeMethods.webrtc_apm_processing_config_destroy(_nativeConfig);
                _nativeConfig = IntPtr.Zero;
            }

            disposedValue = true;
        }
    }

    ~ProcessingConfig()
    {
        Dispose(false);
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    #endregion
}

/// <summary>
/// Represents an APM configuration
/// </summary>
public class ApmConfig : IDisposable
{
    private IntPtr _nativeConfig;

    /// <summary>
    /// Creates a new APM configuration
    /// </summary>
    public ApmConfig()
    {
        _nativeConfig = NativeMethods.webrtc_apm_config_create();
        if (_nativeConfig == IntPtr.Zero)
            throw new InvalidOperationException("Failed to create APM config");
    }

    /// <summary>
    /// Configures the echo canceller
    /// </summary>
    /// <param name="enabled">Whether echo cancellation is enabled</param>
    /// <param name="mobileMode">Whether to use mobile mode</param>
    public void SetEchoCanceller(bool enabled, bool mobileMode)
    {
        NativeMethods.webrtc_apm_config_set_echo_canceller(_nativeConfig, enabled ? 1 : 0, mobileMode ? 1 : 0);
    }

    /// <summary>
    /// Configures noise suppression
    /// </summary>
    /// <param name="enabled">Whether noise suppression is enabled</param>
    /// <param name="level">Noise suppression level</param>
    public void SetNoiseSuppression(bool enabled, NoiseSuppressionLevel level)
    {
        NativeMethods.webrtc_apm_config_set_noise_suppression(_nativeConfig, enabled ? 1 : 0, level);
    }

    /// <summary>
    /// Configures gain controller 1
    /// </summary>
    /// <param name="enabled">Whether gain controller is enabled</param>
    /// <param name="mode">Gain control mode</param>
    /// <param name="targetLevelDbfs">Target level in dBFS</param>
    /// <param name="compressionGainDb">Compression gain in dB</param>
    /// <param name="enableLimiter">Whether to enable the limiter</param>
    public void SetGainController1(bool enabled, GainControlMode mode, int targetLevelDbfs, int compressionGainDb,
        bool enableLimiter)
    {
        NativeMethods.webrtc_apm_config_set_gain_controller1(
            _nativeConfig,
            enabled ? 1 : 0,
            mode,
            targetLevelDbfs,
            compressionGainDb,
            enableLimiter ? 1 : 0);
    }

    /// <summary>
    /// Configures gain controller 2
    /// </summary>
    /// <param name="enabled">Whether gain controller 2 is enabled</param>
    public void SetGainController2(bool enabled)
    {
        NativeMethods.webrtc_apm_config_set_gain_controller2(_nativeConfig, enabled ? 1 : 0);
    }

    /// <summary>
    /// Configures the high pass filter
    /// </summary>
    /// <param name="enabled">Whether the high pass filter is enabled</param>
    public void SetHighPassFilter(bool enabled)
    {
        NativeMethods.webrtc_apm_config_set_high_pass_filter(_nativeConfig, enabled ? 1 : 0);
    }

    /// <summary>
    /// Configures the pre-amplifier
    /// </summary>
    /// <param name="enabled">Whether the pre-amplifier is enabled</param>
    /// <param name="fixedGainFactor">Fixed gain factor</param>
    public void SetPreAmplifier(bool enabled, float fixedGainFactor)
    {
        NativeMethods.webrtc_apm_config_set_pre_amplifier(_nativeConfig, enabled ? 1 : 0, fixedGainFactor);
    }

    /// <summary>
    /// Configures the processing pipeline
    /// </summary>
    /// <param name="maxInternalRate">Maximum internal processing rate</param>
    /// <param name="multiChannelRender">Whether to enable multi-channel render</param>
    /// <param name="multiChannelCapture">Whether to enable multi-channel capture</param>
    /// <param name="downmixMethod">Downmix method</param>
    public void SetPipeline(int maxInternalRate, bool multiChannelRender, bool multiChannelCapture,
        DownmixMethod downmixMethod)
    {
        NativeMethods.webrtc_apm_config_set_pipeline(
            _nativeConfig,
            maxInternalRate,
            multiChannelRender ? 1 : 0,
            multiChannelCapture ? 1 : 0,
            downmixMethod);
    }

    internal IntPtr NativePtr => _nativeConfig;

    #region IDisposable Support

    private bool disposedValue = false;

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (_nativeConfig != IntPtr.Zero)
            {
                NativeMethods.webrtc_apm_config_destroy(_nativeConfig);
                _nativeConfig = IntPtr.Zero;
            }

            disposedValue = true;
        }
    }

    ~ApmConfig()
    {
        Dispose(false);
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    #endregion
}

/// <summary>
/// Provides access to the WebRTC Audio Processing Module
/// </summary>
public class AudioProcessingModule : IDisposable
{
    public IntPtr NativePtr;

    /// <summary>
    /// Creates a new Audio Processing Module instance
    /// </summary>
    public AudioProcessingModule()
    {
        NativePtr = NativeMethods.webrtc_apm_create();
        if (NativePtr == IntPtr.Zero)
            throw new InvalidOperationException("Failed to create APM instance");
    }

    /// <summary>
    /// Applies configuration to the APM
    /// </summary>
    /// <param name="config">Configuration to apply</param>
    /// <returns>Error code</returns>
    public ApmError ApplyConfig(ApmConfig config)
    {
        if (config == null)
            throw new ArgumentNullException(nameof(config));

        return NativeMethods.webrtc_apm_apply_config(NativePtr, config.NativePtr);
    }

    /// <summary>
    /// Initializes the APM with default settings
    /// </summary>
    /// <returns>Error code</returns>
    public ApmError Initialize()
    {
        return NativeMethods.webrtc_apm_initialize(NativePtr);
    }
    
    /// <summary>
    /// Processes a stream of audio data
    /// </summary>
    /// <param name="src">Source audio data (array of channels)</param>
    /// <param name="inputConfig">Input stream configuration</param>
    /// <param name="outputConfig">Output stream configuration</param>
    /// <param name="dest">Destination audio data (array of channels)</param>
    /// <returns>Error code</returns>
    public ApmError ProcessStream(float[][] src, StreamConfig inputConfig, StreamConfig outputConfig, float[][] dest)
    {
        if (src == null || inputConfig == null || outputConfig == null || dest == null)
            throw new ArgumentNullException();

        // Convert float[][] to IntPtr[] for interop
        var srcPtrs = new IntPtr[src.Length];
        var destPtrs = new IntPtr[dest.Length];

        try
        {
            for (int i = 0; i < src.Length; i++)
            {
                srcPtrs[i] = Marshal.AllocHGlobal(src[i].Length * sizeof(float));
                Marshal.Copy(src[i], 0, srcPtrs[i], src[i].Length);
            }

            for (int i = 0; i < dest.Length; i++)
            {
                destPtrs[i] = Marshal.AllocHGlobal(dest[i].Length * sizeof(float));
            }

            // Pin the arrays
            GCHandle srcHandle = GCHandle.Alloc(srcPtrs, GCHandleType.Pinned);
            GCHandle destHandle = GCHandle.Alloc(destPtrs, GCHandleType.Pinned);

            try
            {
                var error = NativeMethods.webrtc_apm_process_stream(
                    NativePtr,
                    srcHandle.AddrOfPinnedObject(),
                    inputConfig.NativePtr,
                    outputConfig.NativePtr,
                    destHandle.AddrOfPinnedObject());

                // Copy results back
                for (int i = 0; i < dest.Length; i++)
                {
                    Marshal.Copy(destPtrs[i], dest[i], 0, dest[i].Length);
                }

                return error;
            }
            finally
            {
                srcHandle.Free();
                destHandle.Free();
            }
        }
        finally
        {
            // Free allocated memory
            for (int i = 0; i < srcPtrs.Length; i++)
            {
                if (srcPtrs[i] != IntPtr.Zero)
                    Marshal.FreeHGlobal(srcPtrs[i]);
            }

            for (int i = 0; i < destPtrs.Length; i++)
            {
                if (destPtrs[i] != IntPtr.Zero)
                    Marshal.FreeHGlobal(destPtrs[i]);
            }
        }
    }

    /// <summary>
    /// Processes a reverse stream of audio data
    /// </summary>
    /// <param name="src">Source audio data (array of channels)</param>
    /// <param name="inputConfig">Input stream configuration</param>
    /// <param name="outputConfig">Output stream configuration</param>
    /// <param name="dest">Destination audio data (array of channels)</param>
    /// <returns>Error code</returns>
    public ApmError ProcessReverseStream(float[][] src, StreamConfig inputConfig, StreamConfig outputConfig,
        float[][] dest)
    {
        if (src == null || inputConfig == null || outputConfig == null || dest == null)
            throw new ArgumentNullException();

        // Convert float[][] to IntPtr[] for interop
        var srcPtrs = new IntPtr[src.Length];
        var destPtrs = new IntPtr[dest.Length];

        try
        {
            for (int i = 0; i < src.Length; i++)
            {
                srcPtrs[i] = Marshal.AllocHGlobal(src[i].Length * sizeof(float));
                Marshal.Copy(src[i], 0, srcPtrs[i], src[i].Length);
            }

            for (int i = 0; i < dest.Length; i++)
            {
                destPtrs[i] = Marshal.AllocHGlobal(dest[i].Length * sizeof(float));
            }

            // Pin the arrays
            GCHandle srcHandle = GCHandle.Alloc(srcPtrs, GCHandleType.Pinned);
            GCHandle destHandle = GCHandle.Alloc(destPtrs, GCHandleType.Pinned);

            try
            {
                var error = NativeMethods.webrtc_apm_process_reverse_stream(
                    NativePtr,
                    srcHandle.AddrOfPinnedObject(),
                    inputConfig.NativePtr,
                    outputConfig.NativePtr,
                    destHandle.AddrOfPinnedObject());

                // Copy results back
                for (int i = 0; i < dest.Length; i++)
                {
                    Marshal.Copy(destPtrs[i], dest[i], 0, dest[i].Length);
                }

                return error;
            }
            finally
            {
                srcHandle.Free();
                destHandle.Free();
            }
        }
        finally
        {
            // Free allocated memory
            for (int i = 0; i < srcPtrs.Length; i++)
            {
                if (srcPtrs[i] != IntPtr.Zero)
                    Marshal.FreeHGlobal(srcPtrs[i]);
            }

            for (int i = 0; i < destPtrs.Length; i++)
            {
                if (destPtrs[i] != IntPtr.Zero)
                    Marshal.FreeHGlobal(destPtrs[i]);
            }
        }
    }

    /// <summary>
    /// Analyzes a reverse stream of audio data
    /// </summary>
    /// <param name="data">Audio data to analyze (array of channels)</param>
    /// <param name="reverseConfig">Reverse stream configuration</param>
    /// <returns>Error code</returns>
    public ApmError AnalyzeReverseStream(float[][] data, StreamConfig reverseConfig)
    {
        if (data == null || reverseConfig == null)
            throw new ArgumentNullException();

        // Convert float[][] to IntPtr[] for interop
        var dataPtrs = new IntPtr[data.Length];

        try
        {
            for (int i = 0; i < data.Length; i++)
            {
                dataPtrs[i] = Marshal.AllocHGlobal(data[i].Length * sizeof(float));
                Marshal.Copy(data[i], 0, dataPtrs[i], data[i].Length);
            }

            // Pin the array
            GCHandle dataHandle = GCHandle.Alloc(dataPtrs, GCHandleType.Pinned);

            try
            {
                return NativeMethods.webrtc_apm_analyze_reverse_stream(
                    NativePtr,
                    dataHandle.AddrOfPinnedObject(),
                    reverseConfig.NativePtr);
            }
            finally
            {
                dataHandle.Free();
            }
        }
        finally
        {
            // Free allocated memory
            for (int i = 0; i < dataPtrs.Length; i++)
            {
                if (dataPtrs[i] != IntPtr.Zero)
                    Marshal.FreeHGlobal(dataPtrs[i]);
            }
        }
    }

    /// <summary>
    /// Sets the analog level for the current stream
    /// </summary>
    /// <param name="level">Analog level (0-255)</param>
    public void SetStreamAnalogLevel(int level)
    {
        NativeMethods.webrtc_apm_set_stream_analog_level(NativePtr, level);
    }

    /// <summary>
    /// Gets the recommended analog level for the current stream
    /// </summary>
    /// <returns>Recommended analog level (0-255)</returns>
    public int GetRecommendedStreamAnalogLevel()
    {
        return NativeMethods.webrtc_apm_recommended_stream_analog_level(NativePtr);
    }

    /// <summary>
    /// Sets the delay in ms between ProcessReverseStream() and ProcessStream()
    /// </summary>
    /// <param name="delayMs">Delay in milliseconds</param>
    public void SetStreamDelayMs(int delayMs)
    {
        NativeMethods.webrtc_apm_set_stream_delay_ms(NativePtr, delayMs);
    }

    /// <summary>
    /// Gets the current stream delay
    /// </summary>
    /// <returns>Current delay in milliseconds</returns>
    public int GetStreamDelayMs()
    {
        return NativeMethods.webrtc_apm_stream_delay_ms(NativePtr);
    }

    /// <summary>
    /// Sets whether a key was pressed during this chunk of audio
    /// </summary>
    /// <param name="keyPressed">Whether a key was pressed</param>
    public void SetStreamKeyPressed(bool keyPressed)
    {
        NativeMethods.webrtc_apm_set_stream_key_pressed(NativePtr, keyPressed ? 1 : 0);
    }

    /// <summary>
    /// Sets whether the output will be muted or in some other way not used
    /// </summary>
    /// <param name="muted">Whether output will be muted</param>
    public void SetOutputWillBeMuted(bool muted)
    {
        NativeMethods.webrtc_apm_set_output_will_be_muted(NativePtr, muted ? 1 : 0);
    }

    /// <summary>
    /// Sets a runtime setting with a float value
    /// </summary>
    /// <param name="type">Type of setting</param>
    /// <param name="value">Float value</param>
    public void SetRuntimeSetting(RuntimeSettingType type, float value)
    {
        NativeMethods.webrtc_apm_set_runtime_setting_float(NativePtr, type, value);
    }

    /// <summary>
    /// Sets a runtime setting with an integer value
    /// </summary>
    /// <param name="type">Type of setting</param>
    /// <param name="value">Integer value</param>
    public void SetRuntimeSetting(RuntimeSettingType type, int value)
    {
        NativeMethods.webrtc_apm_set_runtime_setting_int(NativePtr, type, value);
    }

    /// <summary>
    /// Gets the current processing sample rate
    /// </summary>
    /// <returns>Sample rate in Hz</returns>
    public int GetProcSampleRateHz()
    {
        return NativeMethods.webrtc_apm_proc_sample_rate_hz(NativePtr);
    }

    /// <summary>
    /// Gets the current processing split sample rate
    /// </summary>
    /// <returns>Split sample rate in Hz</returns>
    public int GetProcSplitSampleRateHz()
    {
        return NativeMethods.webrtc_apm_proc_split_sample_rate_hz(NativePtr);
    }

    /// <summary>
    /// Gets the number of input channels
    /// </summary>
    /// <returns>Number of input channels</returns>
    public int GetNumInputChannels()
    {
        return (int)NativeMethods.webrtc_apm_num_input_channels(NativePtr);
    }

    /// <summary>
    /// Gets the number of processing channels
    /// </summary>
    /// <returns>Number of processing channels</returns>
    public int GetNumProcChannels()
    {
        return (int)NativeMethods.webrtc_apm_num_proc_channels(NativePtr);
    }

    /// <summary>
    /// Gets the number of output channels
    /// </summary>
    /// <returns>Number of output channels</returns>
    public int GetNumOutputChannels()
    {
        return (int)NativeMethods.webrtc_apm_num_output_channels(NativePtr);
    }

    /// <summary>
    /// Gets the number of reverse channels
    /// </summary>
    /// <returns>Number of reverse channels</returns>
    public int GetNumReverseChannels()
    {
        return (int)NativeMethods.webrtc_apm_num_reverse_channels(NativePtr);
    }

    /// <summary>
    /// Creates an AEC dump file
    /// </summary>
    /// <param name="fileName">Path to the dump file</param>
    /// <param name="maxLogSizeBytes">Maximum size of the log file in bytes (-1 for unlimited)</param>
    /// <returns>True if successful</returns>
    public bool CreateAecDump(string fileName, long maxLogSizeBytes)
    {
        return NativeMethods.webrtc_apm_create_aec_dump(NativePtr, fileName, maxLogSizeBytes) != 0;
    }

    /// <summary>
    /// Detaches the current AEC dump
    /// </summary>
    public void DetachAecDump()
    {
        NativeMethods.webrtc_apm_detach_aec_dump(NativePtr);
    }

    /// <summary>
    /// Calculates the frame size for a given sample rate
    /// </summary>
    /// <param name="sampleRateHz">Sample rate in Hz</param>
    /// <returns>Frame size in samples</returns>
    public static int GetFrameSize(int sampleRateHz)
    {
        return (int)NativeMethods.webrtc_apm_get_frame_size(sampleRateHz);
    }

    #region IDisposable Support

    private bool disposedValue = false;

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (NativePtr != IntPtr.Zero)
            {
                NativeMethods.webrtc_apm_destroy(NativePtr);
                NativePtr = IntPtr.Zero;
            }

            disposedValue = true;
        }
    }

    ~AudioProcessingModule()
    {
        Dispose(false);
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    #endregion
}

public static partial class NativeMethods
{
    private const string LibraryName = "webrtc-apm";

    static NativeMethods()
    {
        NativeLibrary.SetDllImportResolver(typeof(NativeMethods).Assembly, NativeLibraryResolver.Resolve);
    }

    private static class NativeLibraryResolver
    {
        public static nint Resolve(string libraryName, Assembly assembly, DllImportSearchPath? searchPath)
        {
            // 1. Get the platform-specific library file name (e.g., "libwebrtc-apm.so", "webrtc-apm.dll").
            var platformSpecificName = GetPlatformSpecificLibraryName(libraryName);

            // 2. Try to load the library using its platform-specific name, allowing OS to find it in standard paths.
            if (NativeLibrary.TryLoad(platformSpecificName, assembly, searchPath, out var library))
                return library;

            // 3. If that fails, try to load it from the application's 'runtimes' directory for self-contained apps.
            var relativePath = GetLibraryPath(libraryName);
            var fullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, relativePath);

            if (File.Exists(fullPath) && NativeLibrary.TryLoad(fullPath, out library))
                return library;
            
            // 4. If not found, use Load() to let the runtime throw a detailed DllNotFoundException.
            return NativeLibrary.Load(fullPath); 
        }

        /// <summary>
        /// Gets the platform-specific library name.
        /// </summary>
        private static string GetPlatformSpecificLibraryName(string libraryName)
        {
            if (OperatingSystem.IsWindows())
                return $"{libraryName}.dll";

            if (OperatingSystem.IsMacOS() || OperatingSystem.IsIOS())
                return $"lib{libraryName}.dylib";

            // Default to Linux/Android/FreeBSD convention.
            return $"lib{libraryName}.so";
        }

        /// <summary>
        /// Constructs the relative path to the native library within the 'runtimes' folder.
        /// </summary>
        private static string GetLibraryPath(string libraryName)
        {
            const string relativeBase = "runtimes";
            var platformSpecificName = GetPlatformSpecificLibraryName(libraryName);

            string rid;
            if (OperatingSystem.IsWindows())
            {
                rid = RuntimeInformation.ProcessArchitecture switch
                {
                    Architecture.X86 => "win-x86",
                    Architecture.X64 => "win-x64",
                    _ => throw new PlatformNotSupportedException(
                        $"Unsupported Windows architecture: {RuntimeInformation.ProcessArchitecture}")
                };
            }
            else if (OperatingSystem.IsMacOS())
            {
                rid = RuntimeInformation.ProcessArchitecture switch
                {
                    Architecture.X64 => "osx-x64",
                    Architecture.Arm64 => "osx-arm64",
                    _ => throw new PlatformNotSupportedException(
                        $"Unsupported macOS architecture: {RuntimeInformation.ProcessArchitecture}")
                };
            }
            else if (OperatingSystem.IsLinux())
            {
                rid = RuntimeInformation.ProcessArchitecture switch
                {
                    Architecture.X64 => "linux-x64",
                    Architecture.Arm => "linux-arm",
                    Architecture.Arm64 => "linux-arm64",
                    _ => throw new PlatformNotSupportedException(
                        $"Unsupported Linux architecture: {RuntimeInformation.ProcessArchitecture}")
                };
            }
            else if (OperatingSystem.IsAndroid())
            {
                 rid = RuntimeInformation.ProcessArchitecture switch
                {
                    Architecture.X64 => "android-x64",
                    Architecture.Arm64 => "android-arm64",
                    _ => throw new PlatformNotSupportedException(
                        $"Unsupported Android architecture: {RuntimeInformation.ProcessArchitecture}")
                };
            }
            else if (OperatingSystem.IsIOS())
            {
                rid = RuntimeInformation.ProcessArchitecture switch
                {
                    // iOS uses .framework folders
                    Architecture.Arm64 => "ios-arm64",
                    _ => throw new PlatformNotSupportedException(
                        $"Unsupported iOS architecture: {RuntimeInformation.ProcessArchitecture}")
                };
            }
            else
            {
                throw new PlatformNotSupportedException(
                    $"Unsupported operating system: {RuntimeInformation.OSDescription}");
            }

            return Path.Combine(relativeBase, rid, "native", platformSpecificName);
        }
    }

#pragma warning disable CS1591
    [LibraryImport(LibraryName, EntryPoint = "webrtc_apm_create")]
    public static partial IntPtr webrtc_apm_create();

    [LibraryImport(LibraryName, EntryPoint = "webrtc_apm_destroy")]
    public static partial void webrtc_apm_destroy(IntPtr apm);

    [LibraryImport(LibraryName, EntryPoint = "webrtc_apm_config_create")]
    public static partial IntPtr webrtc_apm_config_create();

    [LibraryImport(LibraryName, EntryPoint = "webrtc_apm_config_destroy")]
    public static partial void webrtc_apm_config_destroy(IntPtr config);

    [LibraryImport(LibraryName, EntryPoint = "webrtc_apm_config_set_echo_canceller")]
    public static partial void webrtc_apm_config_set_echo_canceller(IntPtr config, int enabled, int mobile_mode);

    [LibraryImport(LibraryName, EntryPoint = "webrtc_apm_config_set_noise_suppression")]
    public static partial void webrtc_apm_config_set_noise_suppression(IntPtr config, int enabled,
        NoiseSuppressionLevel level);

    [LibraryImport(LibraryName, EntryPoint = "webrtc_apm_config_set_gain_controller1")]
    public static partial void webrtc_apm_config_set_gain_controller1(IntPtr config, int enabled, GainControlMode mode,
        int target_level_dbfs, int compression_gain_db, int enable_limiter);

    [LibraryImport(LibraryName, EntryPoint = "webrtc_apm_config_set_gain_controller2")]
    public static partial void webrtc_apm_config_set_gain_controller2(IntPtr config, int enabled);

    [LibraryImport(LibraryName, EntryPoint = "webrtc_apm_config_set_high_pass_filter")]
    public static partial void webrtc_apm_config_set_high_pass_filter(IntPtr config, int enabled);

    [LibraryImport(LibraryName, EntryPoint = "webrtc_apm_config_set_pre_amplifier")]
    public static partial void webrtc_apm_config_set_pre_amplifier(IntPtr config, int enabled, float fixed_gain_factor);

    [LibraryImport(LibraryName, EntryPoint = "webrtc_apm_config_set_pipeline")]
    public static partial void webrtc_apm_config_set_pipeline(IntPtr config, int max_internal_rate,
        int multi_channel_render, int multi_channel_capture, DownmixMethod downmix_method);

    [LibraryImport(LibraryName, EntryPoint = "webrtc_apm_apply_config")]
    public static partial ApmError webrtc_apm_apply_config(IntPtr apm, IntPtr config);

    [LibraryImport(LibraryName, EntryPoint = "webrtc_apm_stream_config_create")]
    public static partial IntPtr webrtc_apm_stream_config_create(int sample_rate_hz, UIntPtr num_channels);

    [LibraryImport(LibraryName, EntryPoint = "webrtc_apm_stream_config_destroy")]
    public static partial void webrtc_apm_stream_config_destroy(IntPtr config);

    [LibraryImport(LibraryName, EntryPoint = "webrtc_apm_stream_config_sample_rate_hz")]
    public static partial int webrtc_apm_stream_config_sample_rate_hz(IntPtr config);

    [LibraryImport(LibraryName, EntryPoint = "webrtc_apm_stream_config_num_channels")]
    public static partial UIntPtr webrtc_apm_stream_config_num_channels(IntPtr config);

    [LibraryImport(LibraryName, EntryPoint = "webrtc_apm_processing_config_create")]
    public static partial IntPtr webrtc_apm_processing_config_create();

    [LibraryImport(LibraryName, EntryPoint = "webrtc_apm_processing_config_destroy")]
    public static partial void webrtc_apm_processing_config_destroy(IntPtr config);

    [LibraryImport(LibraryName, EntryPoint = "webrtc_apm_processing_config_input_stream")]
    public static partial IntPtr webrtc_apm_processing_config_input_stream(IntPtr config);

    [LibraryImport(LibraryName, EntryPoint = "webrtc_apm_processing_config_output_stream")]
    public static partial IntPtr webrtc_apm_processing_config_output_stream(IntPtr config);

    [LibraryImport(LibraryName, EntryPoint = "webrtc_apm_processing_config_reverse_input_stream")]
    public static partial IntPtr webrtc_apm_processing_config_reverse_input_stream(IntPtr config);

    [LibraryImport(LibraryName, EntryPoint = "webrtc_apm_processing_config_reverse_output_stream")]
    public static partial IntPtr webrtc_apm_processing_config_reverse_output_stream(IntPtr config);

    [LibraryImport(LibraryName, EntryPoint = "webrtc_apm_initialize")]
    public static partial ApmError webrtc_apm_initialize(IntPtr apm);

    [LibraryImport(LibraryName, EntryPoint = "webrtc_apm_initialize_with_config")]
    public static partial ApmError webrtc_apm_initialize_with_config(IntPtr apm, IntPtr config);

    [LibraryImport(LibraryName, EntryPoint = "webrtc_apm_process_stream")]
    public static partial ApmError webrtc_apm_process_stream(IntPtr apm, IntPtr src, IntPtr input_config,
        IntPtr output_config, IntPtr dest);

    [LibraryImport(LibraryName, EntryPoint = "webrtc_apm_process_reverse_stream")]
    public static partial ApmError webrtc_apm_process_reverse_stream(IntPtr apm, IntPtr src, IntPtr input_config,
        IntPtr output_config, IntPtr dest);

    [LibraryImport(LibraryName, EntryPoint = "webrtc_apm_analyze_reverse_stream")]
    public static partial ApmError webrtc_apm_analyze_reverse_stream(IntPtr apm, IntPtr data, IntPtr reverse_config);

    [LibraryImport(LibraryName, EntryPoint = "webrtc_apm_set_stream_analog_level")]
    public static partial void webrtc_apm_set_stream_analog_level(IntPtr apm, int level);

    [LibraryImport(LibraryName, EntryPoint = "webrtc_apm_recommended_stream_analog_level")]
    public static partial int webrtc_apm_recommended_stream_analog_level(IntPtr apm);

    [LibraryImport(LibraryName, EntryPoint = "webrtc_apm_set_stream_delay_ms")]
    public static partial void webrtc_apm_set_stream_delay_ms(IntPtr apm, int delay);

    [LibraryImport(LibraryName, EntryPoint = "webrtc_apm_stream_delay_ms")]
    public static partial int webrtc_apm_stream_delay_ms(IntPtr apm);

    [LibraryImport(LibraryName, EntryPoint = "webrtc_apm_set_stream_key_pressed")]
    public static partial void webrtc_apm_set_stream_key_pressed(IntPtr apm, int key_pressed);

    [LibraryImport(LibraryName, EntryPoint = "webrtc_apm_set_output_will_be_muted")]
    public static partial void webrtc_apm_set_output_will_be_muted(IntPtr apm, int muted);

    [LibraryImport(LibraryName, EntryPoint = "webrtc_apm_set_runtime_setting_float")]
    public static partial void webrtc_apm_set_runtime_setting_float(IntPtr apm, RuntimeSettingType type, float value);

    [LibraryImport(LibraryName, EntryPoint = "webrtc_apm_set_runtime_setting_int")]
    public static partial void webrtc_apm_set_runtime_setting_int(IntPtr apm, RuntimeSettingType type, int value);

    [LibraryImport(LibraryName, EntryPoint = "webrtc_apm_proc_sample_rate_hz")]
    public static partial int webrtc_apm_proc_sample_rate_hz(IntPtr apm);

    [LibraryImport(LibraryName, EntryPoint = "webrtc_apm_proc_split_sample_rate_hz")]
    public static partial int webrtc_apm_proc_split_sample_rate_hz(IntPtr apm);

    [LibraryImport(LibraryName, EntryPoint = "webrtc_apm_num_input_channels")]
    public static partial UIntPtr webrtc_apm_num_input_channels(IntPtr apm);

    [LibraryImport(LibraryName, EntryPoint = "webrtc_apm_num_proc_channels")]
    public static partial UIntPtr webrtc_apm_num_proc_channels(IntPtr apm);

    [LibraryImport(LibraryName, EntryPoint = "webrtc_apm_num_output_channels")]
    public static partial UIntPtr webrtc_apm_num_output_channels(IntPtr apm);

    [LibraryImport(LibraryName, EntryPoint = "webrtc_apm_num_reverse_channels")]
    public static partial UIntPtr webrtc_apm_num_reverse_channels(IntPtr apm);

    [LibraryImport(LibraryName, EntryPoint = "webrtc_apm_create_aec_dump")]
    public static partial int webrtc_apm_create_aec_dump(IntPtr apm, [MarshalAs(UnmanagedType.LPStr)] string file_name,
        long max_log_size_bytes);

    [LibraryImport(LibraryName, EntryPoint = "webrtc_apm_detach_aec_dump")]
    public static partial void webrtc_apm_detach_aec_dump(IntPtr apm);

    [LibraryImport(LibraryName, EntryPoint = "webrtc_apm_get_frame_size")]
    public static partial UIntPtr webrtc_apm_get_frame_size(int sample_rate_hz);
    
    #pragma warning restore CS1591
}