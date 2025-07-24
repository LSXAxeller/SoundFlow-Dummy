using System.Numerics;
using SoundFlow.Abstracts;
using SoundFlow.Interfaces;
using SoundFlow.Utils;

namespace SoundFlow.Components;

/// <summary>
/// Detects voice activity in audio streams using spectral analysis.
/// </summary>
public class VoiceActivityDetector : AudioAnalyzer
{
    private readonly Queue<float> _sampleBuffer = new();
    private readonly int _fftSize;
    private readonly float[] _window;
    private readonly int _sampleRate;
    private readonly int _channels;
    private bool _isVoiceActive;
    private double _threshold;
    private int _speechLowBand = 300;
    private int _speechHighBand = 3400;

    /// <summary>
    /// Gets whether voice activity is currently detected.
    /// </summary>
    public bool IsVoiceActive
    {
        get => _isVoiceActive;
        private set
        {
            if (_isVoiceActive != value)
            {
                _isVoiceActive = value;
                SpeechDetected?.Invoke(value);
            }
        }
    }

    /// <summary>
    /// Gets or sets the threshold multiplier for voice detection (relative to noise floor).
    /// </summary>
    public double Threshold
    {
        get => _threshold;
        set => _threshold = value;
    }
    

    /// <summary>
    /// Gets or sets the lower bound of the frequency range used for speech detection in Hz.
    /// </summary>
    public int SpeechLowBand
    {
        get => _speechLowBand;
        set => _speechLowBand = value;
    }

    /// <summary>
    /// Gets or sets the upper bound of the frequency range used for speech detection in Hz.
    /// </summary>
    public int SpeechHighBand
    {
        get => _speechHighBand;
        set => _speechHighBand = value;
    }

    /// <summary>
    /// Initializes a new voice activity detector.
    /// </summary>
    /// <param name="fftSize">FFT window size (must be power of two)</param>
    /// <param name="threshold">Detection sensitivity threshold</param>
    /// <param name="visualizer">Optional visualizer for debugging</param>
    /// <remarks>
    /// Increase FFT size for better frequency resolution.
    /// Decrease threshold for higher sensitivity.
    /// Use larger FFT sizes in low-noise environments.
    /// Calibrate threshold based on input levels.
    /// </remarks>
    public VoiceActivityDetector(int fftSize = 1024, float threshold = 0.01f, IVisualizer? visualizer = null)
        : base(visualizer)
    {
        if (!MathHelper.IsPowerOfTwo(fftSize))
            throw new ArgumentException("FFT size must be a power of two", nameof(fftSize));

        _fftSize = fftSize;
        _threshold = threshold;
        _window = MathHelper.HammingWindow(fftSize);
        _sampleRate = AudioEngine.Instance.SampleRate;
        _channels = AudioEngine.Channels;
    }

    /// <summary>
    /// Analyzes audio buffer for voice activity.
    /// </summary>
    protected override void Analyze(Span<float> buffer)
    {
        AddSamplesToBuffer(buffer);
            
        while (_sampleBuffer.Count >= _fftSize)
        {
            var frame = new float[_fftSize];
            for (int i = 0; i < _fftSize; i++)
                frame[i] = _sampleBuffer.Dequeue();

            ApplyWindow(frame);
            var spectrum = ComputeSpectrum(frame);
            var energy = CalculateSpeechBandEnergy(spectrum);
                
            IsVoiceActive = energy > _threshold;
        }
    }

    private void AddSamplesToBuffer(Span<float> buffer)
    {
        if (_channels == 1)
        {
            foreach (var sample in buffer)
                _sampleBuffer.Enqueue(sample);
        }
        else
        {
            for (var i = 0; i < buffer.Length; i += _channels)
            {
                float sum = 0;
                for (var ch = 0; ch < _channels; ch++)
                    sum += buffer[i + ch];
                _sampleBuffer.Enqueue(sum / _channels);
            }
        }
    }

    private void ApplyWindow(Span<float> frame)
    {
        for (var i = 0; i < _fftSize; i++)
            frame[i] *= _window[i];
    }

    private float[] ComputeSpectrum(float[] frame)
    {
        var complexFrame = new Complex[_fftSize];
        for (var i = 0; i < _fftSize; i++)
            complexFrame[i] = new Complex(frame[i], 0);

        MathHelper.Fft(complexFrame);

        var spectrum = new float[_fftSize / 2];
        for (var i = 1; i < _fftSize / 2; i++)
        {
            var magnitude = (float)(complexFrame[i].Magnitude / _fftSize);
            spectrum[i] = magnitude * magnitude;
        }
        return spectrum;
    }
    
    private float CalculateSpeechBandEnergy(float[] spectrum)
    {

            
        var binSize = _sampleRate / (float)_fftSize;
        var lowBin = (int)(_speechLowBand / binSize);
        var highBin = (int)(_speechHighBand / binSize);
            
        highBin = Math.Min(highBin, spectrum.Length - 1);
            
        float energy = 0;
        for (var i = lowBin; i <= highBin; i++)
            energy += spectrum[i];
            
        return energy;
    }
    
    /// <summary>
    /// Occurs when voice activity state changes.
    /// </summary>
    public event Action<bool>? SpeechDetected;
}

