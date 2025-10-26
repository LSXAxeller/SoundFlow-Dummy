namespace SoundFlow.Metadata.Models;

/// <summary>
///     Specifies the desired accuracy vs. speed for duration calculation.
/// </summary>
public enum DurationAccuracy
{
    FastEstimate,
    AccurateScan
}

/// <summary>
///     Configuration options to control the parsing behavior of the AudioReader.
/// </summary>
public sealed class ReadOptions
{
    /// <summary>
    ///     Gets or sets a value indicating whether to parse metadata tags (artist, title, etc.).
    ///     Default is true.
    /// </summary>
    public bool ReadTags { get; set; } = true;

    /// <summary>
    ///     Gets or sets a value indicating whether to parse embedded album art.
    ///     This setting is ignored if ReadTags is false. Default is false.
    /// </summary>
    public bool ReadAlbumArt { get; set; }

    /// <summary>
    ///     Gets or sets a value indicating whether to parse embedded CUE sheets.
    ///     Default is false.
    /// </summary>
    public bool ReadCueSheet { get; set; }

    /// <summary>
    ///     Gets or sets the desired accuracy for duration calculation, especially for VBR files.
    ///     Default is AccurateScan.
    /// </summary>
    public DurationAccuracy DurationAccuracy { get; set; } = DurationAccuracy.AccurateScan;
}