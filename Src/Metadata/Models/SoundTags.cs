namespace SoundFlow.Metadata.Models;

/// <summary>
/// Holds the metadata tags (artist, title, album art, etc.) for an audio file.
/// </summary>
public sealed class SoundTags
{
    public string Title { get; internal set; } = string.Empty;
    public string Artist { get; internal set; } = string.Empty;
    public string Album { get; internal set; } = string.Empty;
    public string Genre { get; internal set; } = string.Empty;
    public uint? Year { get; internal set; }
    public uint? TrackNumber { get; internal set; }
    public byte[]? AlbumArt { get; internal set; }
    
    /// <summary>
    /// Gets the embedded, unsynchronized lyrics. Null if not present.
    /// </summary>
    public string? Lyrics { get; internal set; }

    
    /// <summary>
    /// Converts the SoundTags instance to a human-readable string.
    /// The string will contain the title, artist, album, genre, year, track number, album art size, and lyrics size.
    /// </summary>
    /// <returns>A human-readable string representation of the SoundTags instance.</returns>
    public override string ToString()
    {
        return $"  Title: {Title}\n" +
               $"  Artist: {Artist}\n" +
               $"  Album: {Album}\n" +
               $"  Genre: {Genre}\n" +
               $"  Year: {Year}\n" +
               $"  Track: {TrackNumber}\n" +
               $"  Album Art: {(AlbumArt != null ? $"{AlbumArt.Length} bytes" : "None")}" +
               (Lyrics != null ? $"\n  Lyrics: {(string.IsNullOrWhiteSpace(Lyrics) ? "Present (empty)" : $"{Lyrics.Length} characters")}" : "");
    }
}