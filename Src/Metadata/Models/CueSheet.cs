using System.Text;

namespace SoundFlow.Metadata.Models;

/// <summary>
///     Represents a single track or point within a Cue Sheet.
/// </summary>
public sealed class CuePoint
{
    public uint Id { get; internal set; }
    public ulong PositionSamples { get; internal set; }
    public string Label { get; internal set; } = string.Empty;
    public TimeSpan StartTime { get; internal set; }
}

/// <summary>
///     Represents an embedded Cue Sheet with a collection of cue points.
/// </summary>
public sealed class CueSheet
{
    private readonly List<CuePoint> _cuePoints = new();
    public IReadOnlyList<CuePoint> CuePoints => _cuePoints.AsReadOnly();

    internal void Add(CuePoint point)
    {
        _cuePoints.Add(point);
    }

    internal void Sort()
    {
        _cuePoints.Sort((a, b) => a.PositionSamples.CompareTo(b.PositionSamples));
    }

    public override string ToString()
    {
        var sb = new StringBuilder();
        foreach (var cue in _cuePoints)
            sb.AppendLine($@"  - Track {cue.Id:D2} [{cue.StartTime:hh\:mm\:ss\.fff}]: {cue.Label}");
        return sb.ToString().TrimEnd();
    }
}