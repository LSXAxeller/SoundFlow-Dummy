using SoundFlow.Extensions.WebRtc.Apm.Abstracts;

namespace SoundFlow.Extensions.WebRtc.Apm.Modifiers;

/// <summary>
/// Applies WebRTC Automatic Gain Control (AGC) to an audio stream.
/// Note: This modifier primarily controls the digital AGC modes.
/// Adaptive Analog mode requires external interaction with system microphone gain,
/// which is outside the scope of a standard SoundFlow modifier.
/// </summary>
public class AutomaticGainControlModifier : WebRtcModifierBase
{
    private bool _currentEnabledAgc1;
    private GainControlMode _currentMode;
    private int _currentTargetLevelDbfs;
    private int _currentCompressionGainDb;
    private bool _currentLimiterEnabled;
    private bool _currentEnabledAgc2;
    private bool _currentHighPassFilterEnabled;

    public override string Name { get; set; } = "WebRTC Automatic Gain Control";

    #region AGC1 Properties

    /// <summary>
    /// Gets or sets whether WebRTC Automatic Gain Control (AGC1) is enabled.
    /// </summary>
    public bool Agc1Enabled
    {
        get => _currentEnabledAgc1;
        set
        {
            if (_currentEnabledAgc1 == value) return;
            _currentEnabledAgc1 = value;
            ApplyAgc1Configuration();
        }
    }

    /// <summary>
    /// Gets or sets the mode for AGC1.
    /// Only Digital modes (AdaptiveDigital, FixedDigital) are recommended for use within this modifier.
    /// AdaptiveAnalog requires external system gain control.
    /// </summary>
    /// <remarks>AdaptiveAnalog mode is recommended for Desktop applications.
    /// AdaptiveDigital mode is recommended for Mobile applications.</remarks>
    public GainControlMode Mode
    {
        get => _currentMode;
        set
        {
            if (_currentMode == value) return;
            _currentMode = value;
            ApplyAgc1Configuration();
        }
    }

    /// <summary>
    /// Gets or sets the target peak audio level (in dBFS, typically 0 to -31) for AGC1.
    /// A higher value means a lower target level (e.g., -3 dBFS is louder than -10 dBFS).
    /// </summary>
    public int TargetLevelDbfs
    {
        get => _currentTargetLevelDbfs;
        set
        {
            // Clamp to a reasonable range, e.g., -31 to 0 dBFS
            var clampedValue = Math.Clamp(value, -31, 0);
            if (_currentTargetLevelDbfs == clampedValue) return;
            _currentTargetLevelDbfs = clampedValue;
            ApplyAgc1Configuration();
        }
    }

    /// <summary>
    /// Gets or sets the maximum compression gain (in dB, typically 0 to 90) applied by AGC1 in digital modes.
    /// This limits how much the AGC can boost quiet signals.
    /// </summary>
    public int CompressionGainDb
    {
        get => _currentCompressionGainDb;
        set
        {
            // Clamp to a reasonable range, e.g., 0 to 90 dB
            var clampedValue = Math.Clamp(value, 0, 90);
            if (_currentCompressionGainDb == clampedValue) return;
            _currentCompressionGainDb = clampedValue;
            ApplyAgc1Configuration();
        }
    }

    /// <summary>
    /// Gets or sets whether the built-in limiter is enabled for AGC1.
    /// The limiter prevents clipping after gain is applied.
    /// </summary>
    public bool LimiterEnabled
    {
        get => _currentLimiterEnabled;
        set
        {
            if (_currentLimiterEnabled == value) return;
            _currentLimiterEnabled = value;
            ApplyAgc1Configuration();
        }
    }

    #endregion

    #region AGC2 Properties (Optional)

    /// <summary>
    /// Gets or sets whether the secondary WebRTC Automatic Gain Control (AGC2) is enabled.
    /// This is often used in conjunction with AGC1 or as a simpler fixed gain stage.
    /// </summary>
    public bool Agc2Enabled
    {
        get => _currentEnabledAgc2;
        set
        {
            if (_currentEnabledAgc2 == value) return;
            _currentEnabledAgc2 = value;
            ApplyAgc2Configuration();
        }
    }

    #endregion
    
    #region High Pass Filter Properties

    public bool HighPassFilterEnabled
    {
        get => _currentHighPassFilterEnabled;
        set
        {
            if (_currentHighPassFilterEnabled == value) return;
            _currentHighPassFilterEnabled = value;
            ApplyHighPassFilterConfiguration();
        }
    }

    #endregion


    /// <summary>
    /// Creates a new WebRTC Automatic Gain Control modifier.
    /// The modifier will be disabled if the current SoundFlow AudioEngine sample rate
    /// or channel count is not supported by WebRTC APM.
    /// </summary>
    /// <param name="initiallyEnabledAgc1">Initial enabled state for AGC1.</param>
    /// <param name="initialMode">Initial mode for AGC1 (recommend AdaptiveDigital or FixedDigital).</param>
    /// <param name="initialTargetLevelDbfs">Initial target level (0 to -31 dBFS).</param>
    /// <param name="initialCompressionGainDb">Initial max compression gain (0 to 90 dB).</param>
    /// <param name="initialLimiterEnabled">Initial limiter state for AGC1.</param>
    /// <param name="initiallyEnabledAgc2">Initial enabled state for AGC2.</param>
    public AutomaticGainControlModifier(
        bool initiallyEnabledAgc1 = true,
        GainControlMode initialMode = GainControlMode.AdaptiveDigital,
        int initialTargetLevelDbfs = -3,
        int initialCompressionGainDb = 9,
        bool initialLimiterEnabled = true,
        bool initiallyEnabledAgc2 = true // AGC2 often enabled by default in WebRTC
    )
    {
        _currentEnabledAgc1 = initiallyEnabledAgc1;
        _currentMode = initialMode;
        _currentTargetLevelDbfs = Math.Clamp(initialTargetLevelDbfs, -31, 0);
        _currentCompressionGainDb = Math.Clamp(initialCompressionGainDb, 0, 90);
        _currentLimiterEnabled = initialLimiterEnabled;
        _currentEnabledAgc2 = initiallyEnabledAgc2;

        if (Mode == GainControlMode.AdaptiveAnalog)
        {
            Console.WriteLine("Warning: AdaptiveAnalog AGC mode selected at construction. " +
                              "This mode requires external system gain control.");
        }
    }

    protected override void ConfigureApmFeatures(ApmConfig config)
    {
        // Called by base constructor to set initial feature states
        config.SetGainController1(
            _currentEnabledAgc1,
            _currentMode,
            _currentTargetLevelDbfs,
            _currentCompressionGainDb,
            _currentLimiterEnabled
        );
        config.SetGainController2(_currentEnabledAgc2);
    }

    /// <summary>
    /// Helper method to apply AGC1 settings to the APM configuration and reapply it.
    /// </summary>
    private void ApplyAgc1Configuration()
    {
        if (!IsApmSuccessfullyInitialized || ApmConfig == null) return;
        ApmConfig.SetGainController1(
            _currentEnabledAgc1,
            _currentMode,
            _currentTargetLevelDbfs,
            _currentCompressionGainDb,
            _currentLimiterEnabled
        );
        ReapplyApmConfiguration();
    }

    /// <summary>
    /// Helper method to apply AGC2 settings to the APM configuration and reapply it.
    /// </summary>
    private void ApplyAgc2Configuration()
    {
        if (!IsApmSuccessfullyInitialized || ApmConfig == null) return;
        ApmConfig.SetGainController2(_currentEnabledAgc2);
        ReapplyApmConfiguration();
    }
    
    private void ApplyHighPassFilterConfiguration()
    {
        if (!IsApmSuccessfullyInitialized || ApmConfig == null) return;
        ApmConfig.SetHighPassFilter(_currentHighPassFilterEnabled);
        ReapplyApmConfiguration();
    }
}