using System.Numerics; 
using SoundFlow.Abstracts; 
using SoundFlow.Utils; 
 
namespace SoundFlow.Samples.NoiseSuppression; 
 
/// <summary> 
/// A noise suppression effect that attempts to isolate human speech by attenuating  
/// frequencies outside a typical vocal range. This acts as a band-pass filter. 
/// It uses an FFT to process audio in the frequency domain. 
/// </summary> 
public class VoiceIsolationEffect : SoundModifier 
{ 
    /// <summary> 
    /// The audio sample rate (e.g., 44100 Hz, 48000 Hz). 
    /// This is crucial for correctly mapping FFT bins to frequencies. 
    /// </summary> 
    private readonly int _sampleRate; 
         
    /// <summary> 
    /// The lower bound of the frequency range to preserve (in Hz). 
    /// Frequencies below this will be silenced. 
    /// </summary> 
    public float MinFrequency { get; set; } 
 
    /// <summary> 
    /// The upper bound of the frequency range to preserve (in Hz). 
    /// Frequencies above this will be silenced. 
    /// </summary> 
    public float MaxFrequency { get; set; } 
 
    public override string Name { get; set; } = "Voice Isolation Effect"; 
    public int FftSize { get; private set; } = 2048; // Typical sizes: 1024, 2048, 4096
    public int HopSize { get; private set; } = 512;  // Typically FftSize/4 or FftSize/2
    
    private float[] _window;
    private float[] _overlapBuffer;

    // Modify your constructor:
    public VoiceIsolationEffect(int sampleRate, float minFrequency = 300f, float maxFrequency = 3400f, 
                              int fftSize = 2048, int hopSize = 512)
    {
        _sampleRate = sampleRate;
        MinFrequency = minFrequency;
        MaxFrequency = maxFrequency;
        FftSize = fftSize;
        HopSize = hopSize;
        
        // Initialize window function (Hann window)
        _window = new float[FftSize];
        for (int i = 0; i < FftSize; i++)
        {
            _window[i] = (float)(0.5 * (1 - Math.Cos(2 * Math.PI * i / (FftSize - 1))));
        }
        
        _overlapBuffer = new float[FftSize];
    }

    public override void Process(Span<float> buffer, int channels)
    {
        if (!Enabled || channels <= 0) return;

        for (int c = 0; c < channels; c++)
        {
            ProcessChannel(buffer, c, channels);
        }
    }

    public override float ProcessSample(float sample, int channel)
    {
        throw new NotImplementedException();
    }

    private void ProcessChannel(Span<float> buffer, int channel, int totalChannels)
    {
        int samplesProcessed = 0;
        int frameCount = buffer.Length / totalChannels;
        
        while (samplesProcessed + FftSize <= frameCount)
        {
            // 1. Apply window function and convert to Complex
            var complexBuffer = new Complex[FftSize];
            for (int i = 0; i < FftSize; i++)
            {
                int index = (samplesProcessed + i) * totalChannels + channel;
                complexBuffer[i] = new Complex(buffer[index] * _window[i], 0);
            }

            // 2. Forward FFT
            MathHelper.Fft(complexBuffer);

            // 3. Apply smoother frequency mask
            for (int i = 0; i < FftSize / 2; i++)
            {
                double frequency = (double)i * _sampleRate / FftSize;
                double gain = GetFrequencyGain(frequency);
                
                complexBuffer[i] *= gain;
                if (i > 0)
                {
                    complexBuffer[FftSize - i] *= gain;
                }
            }

            // 4. Inverse FFT
            MathHelper.InverseFft(complexBuffer);

            // 5. Apply window again and overlap-add
            for (int i = 0; i < FftSize; i++)
            {
                int outputIndex = (samplesProcessed + i) * totalChannels + channel;
                float outputSample = (float)complexBuffer[i].Real * _window[i];
                
                // Overlap-add
                if (i < HopSize && samplesProcessed > 0)
                {
                    buffer[outputIndex] = _overlapBuffer[i] + outputSample;
                }
                else
                {
                    buffer[outputIndex] = outputSample;
                }
                
                // Store for next overlap
                if (i >= HopSize)
                {
                    _overlapBuffer[i - HopSize] = outputSample;
                }
            }

            samplesProcessed += HopSize;
        }
    }

    private double GetFrequencyGain(double frequency)
    {
        // Smooth transition instead of hard cut-off
        const double transitionWidth = 100.0; // Hz for transition band
        
        if (frequency < MinFrequency - transitionWidth)
            return 0.0;
        if (frequency > MaxFrequency + transitionWidth)
            return 0.0;
            
        if (frequency >= MinFrequency && frequency <= MaxFrequency)
            return 1.0;
            
        // Transition regions
        if (frequency < MinFrequency)
        {
            double t = (frequency - (MinFrequency - transitionWidth)) / transitionWidth;
            return 0.5 * (1 - Math.Cos(Math.PI * t)); // Raised cosine transition
        }
        else
        {
            double t = ((MaxFrequency + transitionWidth) - frequency) / transitionWidth;
            return 0.5 * (1 - Math.Cos(Math.PI * t)); // Raised cosine transition
        }
    }
}