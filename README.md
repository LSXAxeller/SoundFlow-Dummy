<div align="center">
    <img src="https://raw.githubusercontent.com/LSXPrime/SoundFlow/refs/heads/master/logo.png" alt="Project Logo" width="256" height="256">

# SoundFlow

**A Powerful and Extensible .NET Audio Engine for Enterprise Applications**


[![Build Status](https://github.com/LSXPrime/SoundFlow/actions/workflows/build.yml/badge.svg)](https://github.com/LSXPrime/SoundFlow/actions/workflows/build.yml) [![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT) [![NuGet](https://img.shields.io/nuget/v/SoundFlow.svg)](https://www.nuget.org/packages/SoundFlow) [![.NET](https://img.shields.io/badge/.NET-8.0-blue.svg)](https://dotnet.microsoft.com/download/dotnet/8.0)

</div>


## Introduction

SoundFlow is a robust and versatile .NET audio engine designed for seamless cross-platform audio processing. It provides a comprehensive set of features for audio playback, recording, processing, analysis, and visualization, all within a well-structured and extensible framework.

**Key Features:**

*   **Cross-Platform by Design:**  Runs seamlessly on Windows, macOS, Linux, Android, and iOS or any other .NET compatible platform.
*   **Modular Component Architecture:** Build custom audio pipelines by connecting sources, modifiers, mixers, and analyzers.
*   **Extensibility:** Easily add custom audio components, effects, and visualizers to tailor the engine to your specific needs.
*   **High Performance:** Optimized for real-time audio processing with SIMD support and efficient memory management.
*   **Playback:** Play audio from various sources, including files, streams, and in-memory assets.
*   **Recording:** Capture audio input and save it to different encoding formats.
*   **Mixing:** Combine multiple audio streams with precise control over volume and panning.
*   **Effects:** Apply a wide range of audio effects, including reverb, chorus, delay, equalization, and more.
*   **Analysis:** Extract valuable information from audio data, such as RMS level, peak level, frequency spectrum, and voice activity.
*   **Visualization:** Create engaging visual representations of audio waveforms, spectrums, and level meters.
*   **Surround Sound:** Supports advanced surround sound configurations with customizable speaker positions, delays, and panning methods.
*   **HLS Streaming Support:**  Integrate internet radio and online audio via HTTP Live Streaming.
*   **Backend Agnostic:** Supports the `MiniAudio` backend out of the box, with the ability to add others.

## Getting Started

### Installation

**NuGet Package Manager:**

```
Install-Package SoundFlow
```

**.NET CLI:**

```
dotnet add package SoundFlow
```

### Basic Usage Example

This example demonstrates how to play an audio file using SoundFlow:

```csharp
using SoundFlow.Abstracts;
using SoundFlow.Backends.MiniAudio;
using SoundFlow.Components;
using SoundFlow.Providers;
using SoundFlow.Enums;

// Initialize the audio engine with the MiniAudio backend
using var audioEngine = new MiniAudioEngine(44100, Capability.Playback);

// Create a SoundPlayer and load an audio file
var player = new SoundPlayer(new StreamDataProvider(File.OpenRead("path/to/your/audiofile.wav")));

// Add the player to the master mixer
Mixer.Master.AddComponent(player);

// Start playback
player.Play();

// Keep the console application running until playback finishes
Console.WriteLine("Playing audio... Press any key to stop.");
Console.ReadKey();

// Stop playback and remove the player from the mixer
player.Stop();
Mixer.Master.RemoveComponent(player);
```

## Core Concepts

SoundFlow is built upon a few key concepts:

*   **Audio Engine (`AudioEngine`):** The central component that manages audio device initialization, data processing, and the audio graph.
*   **Sound Components (`SoundComponent`):** Modular units that process or generate audio, forming a directed graph through input and output connections.
*   **Mixer (`Mixer`):** Combines multiple audio streams into a single output.
*   **Sound Modifiers (`SoundModifier`):** Apply audio effects like reverb, chorus, delay, and equalization.
*   **Audio Playback & Recording:** Components for playing and capturing audio, including surround sound support.
*   **Audio Providers:** Standardized way to read audio data from various sources (files, streams, memory).
*   **Audio Analysis & Visualization:** Tools for extracting information from audio and creating visual representations.

**For detailed information on these concepts, please refer to the [SoundFlow Documentation](https://lsxprime.github.io/soundflow-docs/).**

## API Reference

Comprehensive API documentation will be available on the **[SoundFlow Documentation](https://lsxprime.github.io/soundflow-docs/)**.

## Tutorials and Examples

The **[Documentation](https://lsxprime.github.io/soundflow-docs/)** provides a wide range of tutorials and examples to help you get started:

*   **Playback:** Playing audio files and streams, controlling playback.
*   **Recording:** Recording audio, using voice activity detection.
*   **Effects:** Applying various audio effects.
*   **Analysis:** Getting RMS level, analyzing frequency spectrum.
*   **Visualization:** Creating level meters, waveform displays, and spectrum analyzers.

**(Note:** You can also find example code in the `Samples` folder of the repository.)

## Contributing

We welcome contributions to SoundFlow! If you'd like to contribute, please follow these guidelines:

1. **Report Issues:** If you find a bug or have a feature request, please open an issue on the GitHub repository.
2. **Ask Questions and Seek Help:** If you have questions about using SoundFlow or need help with a specific issue, please open a discussion on the GitHub repository to keep the issues section clean and focused.
3. **Submit Pull Requests:**
    *   Fork the repository.
    *   Create a new branch for your changes.
    *   Make your changes, following the project's coding style and conventions.
    *   Make sure your changes is well tested.
    *   Submit a pull request to the `master` branch.
4. **Coding Style:**
    *   Follow the .NET coding conventions.
    *   Use clear and descriptive variable and method names.
    *   Write concise and well-documented code.
    *   Use XML documentation comments for public members.

## Acknowledgments

This project uses the [miniaudio](https://github.com/mackron/miniaudio) library for as default backend for audio I/O.


## Support This Project

SoundFlow is an open-source project driven by passion and community needs.  Maintaining and developing a project of this scale, especially with thorough audio testing, requires significant time and resources.

Currently, development and testing are primarily done using built-in computer speakers.  **Your support will directly help improve the quality of SoundFlow by enabling the purchase of dedicated headphones and audio equipment for more accurate and comprehensive testing across different audio setups.**

Beyond equipment, your contributions, no matter the size, help to:

*   **Dedicate more time to development:** Allowing for faster feature implementation, bug fixes, and improvements.
*   **Enhance project quality:** Enabling better testing, documentation, and overall project stability (including better audio testing with proper equipment!).
*   **Sustain long-term maintenance:** Ensuring SoundFlow remains actively maintained and relevant for the community.

You can directly support SoundFlow and help me get essential headphones through:

*   **AirTM:** For simple one-time donations with various payment options like Direct Bank Transfer (ACH), Debit / Credit Card via Moonpay, Stablecoins, and more than 500 banks and e-wallets.

    [Donate using AirTM](https://airtm.me/lsxprime)


*   **Binance Pay (Crypto - Preferred):**  Support with cryptocurrency via Binance Pay.

    [Binance Pay QR Code/Link](https://app.binance.com/qr/dplk0837ff4256a64749a2b10dfe3ea5a0b9)

    You can also scan this QR code in your Camera app:

[![photo-2025-02-08-13-36-41.jpg](https://i.postimg.cc/02cL1X8K/photo-2025-02-08-13-36-41.jpg)](https://postimg.cc/9rwxGpwc)


**By becoming a sponsor or making a donation, you directly contribute to the future of SoundFlow and help ensure it sounds great for everyone. Thank you for your generosity!**


## License

SoundFlow is released under the [MIT License](LICENSE.md).