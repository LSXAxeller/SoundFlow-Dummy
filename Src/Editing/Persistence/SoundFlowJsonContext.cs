using System.Text.Json.Serialization;
using SoundFlow.Components;
using SoundFlow.Editing.Mapping;
using SoundFlow.Midi.Modifier;
using SoundFlow.Modifiers;
using SoundFlow.Visualization;

namespace SoundFlow.Editing.Persistence;

/// <summary>
/// Source-generated JSON context for SoundFlow project persistence.
/// Includes all DTOs, Primitives, and Built-in Effect types to ensure AOT compatibility.
/// </summary>
[JsonSourceGenerationOptions(
    WriteIndented = true,
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    NumberHandling = JsonNumberHandling.AllowReadingFromString
)]

// Core Project DTOs
[JsonSerializable(typeof(ProjectData))]
[JsonSerializable(typeof(ProjectTrack))]
[JsonSerializable(typeof(ProjectMidiTrack))]
[JsonSerializable(typeof(ProjectSegment))]
[JsonSerializable(typeof(ProjectMidiSegment))]
[JsonSerializable(typeof(ProjectTrackSettings))]
[JsonSerializable(typeof(ProjectAudioSegmentSettings))]
[JsonSerializable(typeof(ProjectSourceReference))]
[JsonSerializable(typeof(ProjectTempoMarker))]
[JsonSerializable(typeof(ProjectEffectData))]
[JsonSerializable(typeof(ProjectMidiMapping))]

// Mapping & Routing Types
[JsonSerializable(typeof(ValueTransformer))]
[JsonSerializable(typeof(MidiInputSource))]
[JsonSerializable(typeof(MidiMappingTarget))]
[JsonSerializable(typeof(MethodArgument))]
[JsonSerializable(typeof(MidiMappingSourceType))]
[JsonSerializable(typeof(MidiMappingBehavior))]
[JsonSerializable(typeof(MidiMappingTargetType))]
[JsonSerializable(typeof(MidiMappingArgumentSource))]
[JsonSerializable(typeof(MidiMappingCurveType))]

// Primitives & Common Enums
[JsonSerializable(typeof(sbyte))]
[JsonSerializable(typeof(short))]
[JsonSerializable(typeof(int))]
[JsonSerializable(typeof(long))]
[JsonSerializable(typeof(byte))]
[JsonSerializable(typeof(ushort))]
[JsonSerializable(typeof(uint))]
[JsonSerializable(typeof(ulong))]
[JsonSerializable(typeof(float))]
[JsonSerializable(typeof(double))]
[JsonSerializable(typeof(decimal))]
[JsonSerializable(typeof(char))]
[JsonSerializable(typeof(bool))]
[JsonSerializable(typeof(string))]
[JsonSerializable(typeof(nint))]
[JsonSerializable(typeof(nuint))]
[JsonSerializable(typeof(Guid))]
[JsonSerializable(typeof(DateTime))]
[JsonSerializable(typeof(DateTimeOffset))]
[JsonSerializable(typeof(TimeSpan))]
[JsonSerializable(typeof(FadeCurveType))]
[JsonSerializable(typeof(LoopSettings))]
[JsonSerializable(typeof(FadeCurveType))]
[JsonSerializable(typeof(LoopSettings))]

// Sound Modifiers
[JsonSerializable(typeof(AlgorithmicReverbModifier))]
[JsonSerializable(typeof(BassBoosterModifier))]
[JsonSerializable(typeof(ChorusModifier))]
[JsonSerializable(typeof(CompressorModifier))]
[JsonSerializable(typeof(DelayModifier))]
[JsonSerializable(typeof(Filter))]
[JsonSerializable(typeof(FrequencyBandModifier))]
[JsonSerializable(typeof(HighPassModifier))]
[JsonSerializable(typeof(LowPassModifier))]
[JsonSerializable(typeof(MultiChannelChorusModifier))]
[JsonSerializable(typeof(ParametricEqualizer))]
[JsonSerializable(typeof(ResamplerModifier))]
[JsonSerializable(typeof(TrebleBoosterModifier))]
[JsonSerializable(typeof(VocalExtractorModifier))]

// Modifier Specific Sub-Types
[JsonSerializable(typeof(Filter.FilterType))] 
[JsonSerializable(typeof(EqualizerBand))]
[JsonSerializable(typeof(List<EqualizerBand>))]
[JsonSerializable(typeof(FilterType))]

// Audio Analyzers
[JsonSerializable(typeof(LevelMeterAnalyzer))]
[JsonSerializable(typeof(SpectrumAnalyzer))]
[JsonSerializable(typeof(VoiceActivityDetector))]

// MIDI Modifiers
[JsonSerializable(typeof(ArpeggiatorModifier))]
[JsonSerializable(typeof(ChannelFilterModifier))]
[JsonSerializable(typeof(HarmonizerModifier))]
[JsonSerializable(typeof(RandomizerModifier))]
[JsonSerializable(typeof(TransposeModifier))]
[JsonSerializable(typeof(VelocityModifier))]

// MIDI Modifier Sub-Types
[JsonSerializable(typeof(ArpMode))]
[JsonSerializable(typeof(int[]))] // For Harmonizer intervals
internal partial class SoundFlowJsonContext : JsonSerializerContext;