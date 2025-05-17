using SoundFlow.Extensions.WebRtc.Apm.Abstracts;

namespace SoundFlow.Extensions.WebRtc.Apm.Modifiers;

public class NoiseSuppressionModifier : WebRtcModifierBase
{
    private bool _currentEnabledNs;
    private NoiseSuppressionLevel _currentLevel;

    public override string Name { get; set; } = "WebRTC Noise Suppression";

    /// <summary>
    /// Gets or sets whether WebRTC Noise Suppression is enabled.
    /// </summary>
    public bool NoiseSuppressionEnabled
    {
        get => _currentEnabledNs;
        set
        {
            if (_currentEnabledNs == value) return;
            _currentEnabledNs = value;
            if (IsApmSuccessfullyInitialized && ApmConfig != null)
            {
                ApmConfig.SetNoiseSuppression(_currentEnabledNs, _currentLevel);
                ReapplyApmConfiguration();
            }
        }
    }

    /// <summary>
    /// Gets or sets the strength level of the WebRTC Noise Suppression.
    /// </summary>
    public NoiseSuppressionLevel SuppressionLevel
    {
        get => _currentLevel;
        set
        {
            if (_currentLevel == value) return;
            _currentLevel = value;
            if (IsApmSuccessfullyInitialized && ApmConfig != null)
            {
                ApmConfig.SetNoiseSuppression(_currentEnabledNs, _currentLevel);
                ReapplyApmConfiguration();
            }
        }
    }

    /// <summary>
    /// Creates a new WebRTC Noise Suppression modifier.
    /// The modifier will be disabled if the current SoundFlow AudioEngine sample rate
    /// is not supported by WebRTC APM.
    /// </summary>
    /// <param name="initiallyEnabled">Initial enabled state for noise suppression.</param>
    /// <param name="initialLevel">Initial noise suppression strength level.</param>
    public NoiseSuppressionModifier(bool initiallyEnabled = true, NoiseSuppressionLevel initialLevel = NoiseSuppressionLevel.High)
    {
        _currentEnabledNs = initiallyEnabled;
        _currentLevel = initialLevel;
    }

    protected override void ConfigureApmFeatures(ApmConfig config)
    {
        config.SetNoiseSuppression(_currentEnabledNs, _currentLevel);
    }
}