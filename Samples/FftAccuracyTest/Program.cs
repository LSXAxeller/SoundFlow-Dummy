using System.Diagnostics;
using System.Numerics;
using FftSharp;
using SoundFlow.Utils;

namespace FftAccuracyTest;

internal static class Program
{
    private const double ToleranceDouble = 1e-9;
    private const float ToleranceFloat = 1e-6f;

    // This counter is reset for each test suite run.
    private static int _failureCount;
    private static long _testsRunInSuite;

    private static void Main()
    {
        Console.WriteLine("--- Comprehensive Fuzz Test Suite for MathHelper ---");
        Console.WriteLine("Running tests for 30 seconds per configuration...");

        var totalFailures = 0L;
        var totalTests = 0L;

        // Define the test configurations
        var configurations = new[]
        {
            (Name: "Default (AVX & SSE Enabled)", Avx: true, Sse: true),
            (Name: "AVX Enabled, SSE Disabled",   Avx: true, Sse: false),
            (Name: "SSE Enabled, AVX Disabled",   Avx: false, Sse: true),
            (Name: "Scalar (AVX & SSE Disabled)", Avx: false, Sse: false)
        };

        foreach (var config in configurations)
        {
            Console.WriteLine($"\n\n=======================================================");
            Console.WriteLine($"   STARTING TEST RUN: {config.Name}");
            Console.WriteLine($"=======================================================");
            
            // Set the static properties on MathHelper for this run
            MathHelper.EnableAvx = config.Avx;
            MathHelper.EnableSse = config.Sse;

            // Run the entire suite with the current configuration
            RunFuzzingSuite(TimeSpan.FromSeconds(30));
            totalFailures += _failureCount;
            totalTests += _testsRunInSuite;
        }

        Console.WriteLine("\n\n--- Overall Test Run Summary ---");
        Console.WriteLine($"Completed {totalTests:N0} total test iterations.");
        if (totalFailures == 0)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("All tests passed successfully across all configurations!");
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"A total of {totalFailures} test(s) failed across all configurations.");
        }
        Console.ResetColor();
    }

    /// <summary>
    /// Executes a fuzzing test suite for a given duration.
    /// </summary>
    private static void RunFuzzingSuite(TimeSpan duration)
    {
        // Reset counters for this specific run
        _failureCount = 0; 
        _testsRunInSuite = 0;

        var random = new Random();
        var stopwatch = Stopwatch.StartNew();
        
        // Always run the static, known-edge-case tests first.
        Console.WriteLine("\n[Running Static Edge-Case Tests]");
        TestIsPowerOfTwo(); // This is better as a static test
        TestLerp(null);     // Run static portion
        TestMod(null);      // Run static portion
        TestPrincipalAngle(null); // Run static portion
        Console.WriteLine("\n[Running Randomized Fuzz Tests]");

        while (stopwatch.Elapsed < duration)
        {
            // Generate random parameters for this iteration
            // FFT size must be a power of two. e.g., 32, 64, ..., 4096
            int fftSize = 1 << random.Next(5, 13); 
            // Window size can be anything. Let's test odd and even sizes.
            int windowSize = random.Next(2, 2050);

            // Run tests with randomized data
            TestFftAndIfft(random, fftSize);
            TestWindowFunctions(random, windowSize);
            TestLerp(random);
            TestMod(random);
            TestPrincipalAngle(random);
            
            _testsRunInSuite++;
            if (_testsRunInSuite % 100 == 0) Console.Write(".");
        }
        stopwatch.Stop();
        Console.WriteLine(); // Newline after the progress dots

        Console.WriteLine($"\n--- Configuration Run Summary ({stopwatch.Elapsed.TotalSeconds:F2}s) ---");
        Console.WriteLine($"Executed {_testsRunInSuite:N0} randomized test iterations.");
        if (_failureCount == 0)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("All tests in this configuration passed.");
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"{_failureCount} test(s) in this configuration failed.");
        }
        Console.ResetColor();
    }

    private static void RunTest(string testName, bool success, string details = "")
    {
        // Only print successes for the initial static tests, not for every fuzz iteration
        var isFuzzTest = new StackTrace().GetFrame(1)!.GetMethod()!.Name != nameof(RunFuzzingSuite);
        
        if (success && isFuzzTest)
        {
            return; // Don't flood the console on successful fuzz tests
        }
        
        Console.Write($"\n  - {testName}: ");
        if (success)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("SUCCESS");
        }
        else
        {
            _failureCount++;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("FAILURE");
            if (!string.IsNullOrEmpty(details))
            {
                Console.WriteLine($"      -> {details}");
            }
        }
        Console.ResetColor();
    }

    private static void TestWindowFunctions(Random random, int size)
    {
        // --- Hamming Window Test ---
        var hammingSimd = MathHelper.HammingWindow(size);
        var hammingScalar = WindowFunctions.Hamming(size);
        float maxHammingDiff = 0;
        
        for(int i = 0; i < size; i++)
        {
            maxHammingDiff = Math.Max(maxHammingDiff, Math.Abs(hammingSimd[i] - hammingScalar[i]));
        }
        RunTest("Hamming Window", maxHammingDiff < ToleranceFloat, $"Size={size}, MaxDiff={maxHammingDiff:E}");
        RunTest("Hamming Window edge case (size=1)", Math.Abs(MathHelper.HammingWindow(1)[0] - 1.0f) < ToleranceFloat);

        // --- Hanning Window Test ---
        var hanningSimd = MathHelper.HanningWindow(size);
        var hanningScalar = WindowFunctions.Hanning(size);
        float maxHanningDiff = 0;
        
        for (int i = 0; i < size; i++)
        {
            maxHanningDiff = Math.Max(maxHanningDiff, Math.Abs(hanningSimd[i] - hanningScalar[i]));
        }
        RunTest("Hanning Window", maxHanningDiff < ToleranceFloat, $"Size={size}, MaxDiff={maxHanningDiff:E}");
        RunTest("Hanning Window edge case (size=1)", Math.Abs(MathHelper.HanningWindow(1)[0] - 1.0f) < ToleranceFloat);
    }
    
    private static void TestFftAndIfft(Random random, int fftSize)
    {
        var originalData = GenerateTestData(fftSize, 48000, random);

        // Test Forward FFT
        var dataForMathHelper = (Complex[])originalData.Clone();
        var dataForFftSharp = (Complex[])originalData.Clone();
        MathHelper.Fft(dataForMathHelper);
        FFT.Forward(dataForFftSharp);
        
        double maxFftDiff = 0;
        for (int i = 0; i < fftSize; i++)
        {
            maxFftDiff = Math.Max(maxFftDiff, (dataForMathHelper[i] - dataForFftSharp[i]).Magnitude);
        }
        RunTest("FFT accuracy vs. FftSharp", maxFftDiff < ToleranceDouble, $"Size={fftSize}, MaxDiff={maxFftDiff:E}");

        // Test Inverse FFT
        // We use the result from our FFT to test our IFFT
        MathHelper.InverseFft(dataForMathHelper);

        double maxIfftDiff = 0;
        for (int i = 0; i < fftSize; i++)
        {
            maxIfftDiff = Math.Max(maxIfftDiff, (originalData[i] - dataForMathHelper[i]).Magnitude);
        }
        RunTest("IFFT restores original signal", maxIfftDiff < ToleranceDouble, $"Size={fftSize}, MaxDiff={maxIfftDiff:E}");
    }
    
    // if random is null, runs static tests. Otherwise, runs randomized tests.
    private static void TestLerp(Random? random)
    {
        if (random is null)
        {
            var a = 10f;
            var b = 20f;
            bool success = true;
            success &= Math.Abs(MathHelper.Lerp(a, b, 0f) - 10f) < ToleranceFloat;
            success &= Math.Abs(MathHelper.Lerp(a, b, 1f) - 20f) < ToleranceFloat;
            success &= Math.Abs(MathHelper.Lerp(a, b, 0.5f) - 15f) < ToleranceFloat;
            success &= Math.Abs(MathHelper.Lerp(a, b, -0.5f) - 10f) < ToleranceFloat; // Clamping
            success &= Math.Abs(MathHelper.Lerp(a, b, 1.5f) - 20f) < ToleranceFloat; // Clamping
            RunTest("Linear Interpolation (Lerp) [Static]", success);
        }
        else
        {
            float a = (random.NextSingle() - 0.5f) * 1000f;
            float b = (random.NextSingle() - 0.5f) * 1000f;
            float t = (random.NextSingle() - 0.25f) * 1.5f; // Test clamping and in-range
            float expected = Math.Clamp(t, 0f, 1f) * (b - a) + a;
            float actual = MathHelper.Lerp(a, b, t);
            bool success = Math.Abs(expected - actual) < ToleranceFloat;
            RunTest("Linear Interpolation (Lerp) [Random]", success, $"Lerp({a}, {b}, {t}) -> Expected: {expected}, Got: {actual}");
        }
    }

    private static void TestIsPowerOfTwo()
    {
        long[] powers = { 1, 2, 4, 8, 16, 1024, 65536 };
        long[] notPowers = { 0, -2, 3, 5, 100, 1023 };
        bool success = true;
        foreach (var p in powers) success &= MathHelper.IsPowerOfTwo(p);
        foreach (var np in notPowers) success &= !MathHelper.IsPowerOfTwo(np);
        RunTest("Power of Two check", success);
    }
    
    private static void TestMod(Random? random)
    {
        if (random is null)
        {
            bool success = true;
            success &= Math.Abs(5.5.Mod(2.0) - 1.5) < ToleranceDouble;
            success &= Math.Abs((-1.5).Mod(2.0) - 0.5) < ToleranceDouble; // Key test case vs %
            success &= Math.Abs(4.0.Mod(2.0) - 0.0) < ToleranceDouble;
            success &= Math.Abs((-4.0).Mod(2.0) - 0.0) < ToleranceDouble;
            RunTest("Modulus behavior [Static]", success);
        }
        else
        {
            double val = (random.NextDouble() - 0.5) * 200.0;
            double mod = random.NextDouble() * 20.0 + 1.0; // Ensure mod > 0
            double expected = ((val % mod) + mod) % mod;
            double actual = val.Mod(mod);
            bool success = Math.Abs(expected - actual) < ToleranceDouble;
            RunTest("Modulus behavior [Random]", success, $"{val}.Mod({mod}) -> Expected: {expected}, Got: {actual}");
        }
    }

    private static void TestPrincipalAngle(Random? random)
    {
        if (random is null)
        {
            bool success = true;
            success &= Math.Abs(MathHelper.PrincipalAngle(MathF.PI / 2) - (MathF.PI / 2)) < ToleranceFloat;
            success &= Math.Abs(MathHelper.PrincipalAngle(MathF.PI + 0.1f) - (-MathF.PI + 0.1f)) < ToleranceFloat;
            success &= Math.Abs(MathHelper.PrincipalAngle(3 * MathF.PI) - (-MathF.PI)) < ToleranceFloat;
            success &= Math.Abs(MathHelper.PrincipalAngle(-3.5f * MathF.PI) - (0.5f * MathF.PI)) < ToleranceFloat;
            success &= Math.Abs(MathHelper.PrincipalAngle(MathF.PI) - (-MathF.PI)) < ToleranceFloat;
            RunTest("Angle wrapping to [-PI, PI) [Static]", success);
        }
        else
        {
            float angle = (random.NextSingle() - 0.5f) * 20 * MathF.PI; // Test a wide range of angles
            float actual = MathHelper.PrincipalAngle(angle);
            bool success = actual > -MathF.PI - ToleranceFloat && actual <= MathF.PI + ToleranceFloat;
            // A more precise check: the result should be congruent to the original angle mod 2*PI
            float diff = (angle - actual) / (2 * MathF.PI);
            success &= Math.Abs(diff - MathF.Round(diff)) < ToleranceFloat * 10; // Allow slightly larger tolerance for float congruence
            RunTest("Angle wrapping to [-PI, PI) [Random]", success, $"Angle: {angle}, Result: {actual}");
        }
    }

    private static Complex[] GenerateTestData(int size, int sampleRate, Random random)
    {
        var data = new Complex[size];
        int numSines = random.Next(1, 5); // Combine 1 to 4 sine waves

        for (int s = 0; s < numSines; s++)
        {
            double freq = random.NextDouble() * (sampleRate / 2.0); // Freq up to Nyquist
            double amp = random.NextDouble() * 2.0 - 1.0; // Amplitude between -1 and 1
            
            for (var i = 0; i < size; i++)
            {
                var time = (double)i / sampleRate;
                double sample = amp * Math.Sin(2 * Math.PI * freq * time);
                data[i] = new Complex(data[i].Real + sample, 0); // Add to existing signal
            }
        }
        return data;
    }
}


/// <summary>
/// Provides static methods for generating common signal processing window functions.
/// This is the "ground truth" implementation used for comparison.
/// </summary>
public static class WindowFunctions
{
    public static float[] Hamming(int size) => CreateWindow(size, 0.54f, 0.46f);
    public static float[] Hanning(int size) => CreateWindow(size, 0.5f, 0.5f);
    
    private static float[] CreateWindow(int size, float alpha, float beta)
    {
        if (size <= 0) return [];
        if (size == 1) return [1.0f];

        float[] window = new float[size];
        float denominator = size - 1;

        for (int n = 0; n < size; n++)
        {
            window[n] = alpha - beta * MathF.Cos((2 * MathF.PI * n) / denominator);
        }
        
        return window;
    }
}