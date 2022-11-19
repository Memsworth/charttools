﻿using ChartTools.IO;
using ChartTools.IO.Chart;
using ChartTools.IO.Configuration;
using ChartTools.IO.Formatting;

namespace ChartTools;

public record GHLInstrument : Instrument<GHLChord>
{
    public new GHLInstrumentIdentity InstrumentIdentity { get; init; }

    public GHLInstrument() { }
    public GHLInstrument(GHLInstrumentIdentity identity) => InstrumentIdentity = identity;

    protected override InstrumentIdentity GetIdentity() => (InstrumentIdentity)InstrumentIdentity;

    #region File reading
    /// <summary>
    /// Reads a GHL instrument from a file.
    /// </summary>
    public static GHLInstrument? FromFile(string path, GHLInstrumentIdentity instrument, ReadingConfiguration? config = default, FormattingRules? formatting = default) => ExtensionHandler.Read(path, (".chart", path => ChartFile.ReadInstrument(path, instrument, config, formatting)));
    /// <summary>
    /// Reads a GHL instrument from a file asynchronously using multitasking.
    /// </summary>
    public static async Task<GHLInstrument?> FromFileAsync(string path, GHLInstrumentIdentity instrument, ReadingConfiguration? config = default, FormattingRules? formatting = default, CancellationToken cancellationToken = default) => await ExtensionHandler.ReadAsync(path, (".chart", path => ChartFile.ReadInstrumentAsync(path, instrument, config, formatting, cancellationToken)));

    public static DirectoryResult<GHLInstrument?> FromDirectory(string directory, GHLInstrumentIdentity instrument, ReadingConfiguration? config = default) => DirectoryHandler.FromDirectory(directory, (path, formatting) => FromFile(path, instrument, config, formatting));
    public static Task<DirectoryResult<GHLInstrument?>> FromDirectoryAsync(string directory, GHLInstrumentIdentity instrument, ReadingConfiguration? config = default, CancellationToken cancellationToken = default) => DirectoryHandler.FromDirectoryAsync(directory, async (path, formatting) => await FromFileAsync(path, instrument, config, formatting, cancellationToken), cancellationToken);
    #endregion
}