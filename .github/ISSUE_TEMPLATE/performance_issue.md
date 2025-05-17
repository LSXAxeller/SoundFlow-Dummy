---
name: "⏱️ Performance Issue"
about: Report a performance problem with SoundFlow
title: "[PERF] Brief description of the performance issue"
labels: ''
assignees: ''

---

**Thank you for reporting a performance issue! Accurate details are key to helping us optimize SoundFlow.**

**1. Describe the Performance Issue**

A clear and concise description of the performance problem.
*   Is it high CPU usage, high memory consumption, audio stuttering/glitches, slow initialization, etc.?

**2. Area of the Library Affected**

Which part of SoundFlow seems to be causing the performance issue?
*   [e.g., `SoundPlayer` with many instances, a specific `SoundModifier`, `MiniAudioEngine` processing, `NetworkDataProvider` streaming]

**3. Scenario Causing Slowness**

Describe the specific scenario or operations that trigger the performance problem.
*   Please provide steps if possible.

**4. Expected Performance**

What level of performance were you expecting in this scenario?
*   [e.g., "CPU usage below 10%", "No audible glitches", "Modifier should process X samples in Y ms"]

**5. Actual Performance**

Describe the observed performance. Provide metrics if possible:
*   **CPU Usage:** [e.g., "Sustained 80% on one core", "Spikes to 100%"]
*   **Memory Usage:** [e.g., "Increases by 100MB per minute", "Consumes 2GB RAM"]
*   **Latency/Glitches:** [e.g., "Audio stutters every 5 seconds", "Noticeable delay in processing"]
*   **Timing:** [e.g., "Method X takes 500ms to complete for a 1s buffer"]

**6. Minimal Reproducible Example (MRE) (Highly Recommended)**

Please provide a *minimal* code snippet that demonstrates the performance issue. This is extremely helpful.
```csharp
// Your MRE code here
```

**7. Profiling Data (if available)**

If you have run a profiler (e.g., dotTrace, PerfView, Visual Studio Profiler), please share:
*   Screenshots of hot paths.
*   Exported profiling sessions (if shareable).
*   Key findings from the profiler.

**8. Environment (please complete the following information):**
*   **SoundFlow Version:** [e.g., 1.0.0, or commit SHA]
*   **.NET Version:** [e.g., .NET 8.0]
*   **Operating System:** [e.g., Windows 11, macOS Sonoma, Ubuntu 22.04]
*   **Architecture:** [e.g., x64, ARM64]
*   **Audio Backend Used (if known):** [e.g., MiniAudioEngine with WASAPI, CoreAudio, ALSA]
*   **Specific Audio Hardware (if relevant):** [e.g., CPU model, RAM amount, Audio Interface model]
*   **Audio Buffer Size / Sample Rate:** [e.g., 480 samples / 48000 Hz]

**9. Additional Context**

Add any other relevant information.
*   Does the issue scale with input size, number of components, etc.?
*   Have you tried different configurations or settings?

**Requirements:**
*   [ ] I have searched the existing issues to ensure this performance issue has not already been reported.
*   [ ] I have provided a clear description of the issue and the scenario.
*   [ ] I have included performance metrics if possible.
*   [ ] I have considered providing an MRE.
*   [ ] I have completed the environment information.
