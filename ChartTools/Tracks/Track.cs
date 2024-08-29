﻿using ChartTools.Events;
using ChartTools.IO;
using ChartTools.IO.Chart;
using ChartTools.IO.Configuration;
using ChartTools.IO.Formatting;

using System.Diagnostics;

namespace ChartTools;

/// <summary>
/// Base class for tracks
/// </summary>
[DebuggerDisplay("{Difficulty}")]
public abstract record Track : IEmptyVerifiable
{
    /// <inheritdoc cref="IEmptyVerifiable.IsEmpty"/>
    public bool IsEmpty => Chords.Count == 0 && LocalEvents.Count == 0 && SpecialPhrases.Count == 0;

    /// <summary>
    /// Difficulty of the track
    /// </summary>
    public Difficulty Difficulty { get; init; }

    /// <summary>
    /// Instrument containing the track
    /// </summary>
    public Instrument? ParentInstrument => GetInstrument();

    /// <summary>
    /// Events specific to the <see cref="Track"/>
    /// </summary>
    public List<LocalEvent> LocalEvents { get; } = [];

    /// <summary>
    /// Set of special phrases
    /// </summary>
    public List<TrackSpecialPhrase> SpecialPhrases { get; } = [];

    /// <summary>
    /// Groups of notes of the same position
    /// </summary>
    public IReadOnlyList<IChord> Chords => GetChords();

    protected abstract IReadOnlyList<IChord> GetChords();

    internal IEnumerable<TrackSpecialPhrase> SoloToStarPower(bool removeEvents)
    {
        if (LocalEvents is null)
            yield break;

        foreach (LocalEvent e in LocalEvents.OrderBy(e => e.Position))
        {
            TrackSpecialPhrase? phrase = null;

            switch (e.EventType)
            {
                case EventTypeHelper.Local.Solo:
                    phrase = new(e.Position, TrackSpecialPhraseType.StarPowerGain);
                    break;
                case EventTypeHelper.Local.SoloEnd:
                    if (phrase is not null)
                    {
                        phrase.Length = e.Position - phrase.Position;
                        yield return phrase;
                        phrase = null;
                    }
                    break;
            }
        }

        if (removeEvents)
            LocalEvents.RemoveAll(e => e.IsSoloEvent);
    }

    protected abstract Instrument? GetInstrument();

    #region File reading
    #region Single file
    [Obsolete($"Use {nameof(ChartFile.ReadInstruments)} with a component list.")]
    public static Track FromFile(string path, InstrumentIdentity instrument, Difficulty difficulty, ReadingConfiguration? config = default, FormattingRules? formatting = default) => ExtensionHandler.Read(path, (".chart", path => ChartFile.ReadTrack(path, instrument, difficulty, config?.Chart, formatting)));

    [Obsolete($"Use {nameof(ChartFile.ReadInstrumentsAsync)} with a component list.")]
    public static async Task<Track> FromFileAsync(string path, InstrumentIdentity instrument, Difficulty difficulty, ReadingConfiguration? config = default, FormattingRules? formatting = default, CancellationToken cancellationToken = default) => await ExtensionHandler.ReadAsync(path, (".chart", path => ChartFile.ReadTrackAsync(path, instrument, difficulty, config?.Chart, formatting, cancellationToken)));

    [Obsolete($"Use {nameof(ChartFile.ReadInstruments)} with a component list.")]
    public static Track<DrumsChord> FromFile(string path, Difficulty difficulty, ReadingConfiguration? config = default, FormattingRules? formatting = default) => ExtensionHandler.Read(path, (".chart", path => ChartFile.ReadDrumsTrack(path, difficulty, config?.Chart, formatting)));

    [Obsolete($"Use {nameof(ChartFile.ReadInstrumentsAsync)} with a component list.")]
    public static async Task<Track<DrumsChord>> FromFileAsync(string path, Difficulty difficulty, ReadingConfiguration? config = default, FormattingRules? formatting = default, CancellationToken cancellationToken = default) => await ExtensionHandler.ReadAsync(path, (".chart", path => ChartFile.ReadDrumsTrackAsync(path, difficulty, config?.Chart, formatting, cancellationToken)));

    [Obsolete($"Use {nameof(ChartFile.ReadInstruments)} with a component list.")]
    public static Track<GHLChord> FromFile(string path, GHLInstrumentIdentity instrument, Difficulty difficulty, ReadingConfiguration? config = default, FormattingRules? formatting = default) => ExtensionHandler.Read(path, (".chart", path => ChartFile.ReadTrack(path, instrument, difficulty, config?.Chart, formatting)));

    [Obsolete($"Use {nameof(ChartFile.ReadInstrumentsAsync)} with a component list.")]
    public static async Task<Track<GHLChord>> FromFileAsync(string path, GHLInstrumentIdentity instrument, Difficulty difficulty, ReadingConfiguration? config = default, FormattingRules? formatting = default, CancellationToken cancellationToken = default) => await ExtensionHandler.ReadAsync(path, (".chart", path => ChartFile.ReadTrackAsync(path, instrument, difficulty, config?.Chart, formatting, cancellationToken)));

    [Obsolete($"Use {nameof(ChartFile.ReadInstruments)} with a component list.")]
    public static Track<StandardChord> FromFile(string path, StandardInstrumentIdentity instrument, Difficulty difficulty, ReadingConfiguration? config = default, FormattingRules? formatting = default) => ExtensionHandler.Read(path, (".chart", path => ChartFile.ReadTrack(path, instrument, difficulty, config?.Chart, formatting)));

    [Obsolete($"Use {nameof(ChartFile.ReadInstrumentsAsync)} with a component list.")]
    public static async Task<Track<StandardChord>> FromFileAsync(string path, StandardInstrumentIdentity instrument, Difficulty difficulty, ReadingConfiguration? config = default, FormattingRules? formatting = default, CancellationToken cancellationToken = default) => await ExtensionHandler.ReadAsync(path, (".chart", path => ChartFile.ReadTrackAsync(path, instrument, difficulty, config?.Chart, formatting, cancellationToken)));
    #endregion

    #region Directory
    [Obsolete($"Use {nameof(ChartFile.ReadInstruments)} with a component list and {nameof(Metadata.Formatting)}.")]
    public static DirectoryResult<Track?> FromDirectory(string directory, InstrumentIdentity instrument, Difficulty difficulty, ReadingConfiguration? config = default) => DirectoryHandler.FromDirectory(directory, (path, formatting) => FromFile(path, instrument, difficulty, config, formatting));

    [Obsolete($"Use {nameof(ChartFile.ReadInstrumentsAsync)} with a component list and {nameof(Metadata.Formatting)}.")]
    public static async Task<DirectoryResult<Track?>> FromDirectoryAsync(string directory, InstrumentIdentity instrument, Difficulty difficulty, ReadingConfiguration? config = default, CancellationToken cancellationToken = default) => await DirectoryHandler.FromDirectoryAsync(directory, async (path, formatting) => await FromFileAsync(path, instrument, difficulty, config, formatting, cancellationToken), cancellationToken);

    [Obsolete($"Use {nameof(ChartFile.ReadInstruments)} with a component list and {nameof(Metadata.Formatting)}.")]
    public static DirectoryResult<Track<DrumsChord>?> FromDirectory(string directory, Difficulty difficulty, ReadingConfiguration? config = default) => DirectoryHandler.FromDirectory(directory, (path, formatting) => FromFile(path, difficulty, config, formatting));

    [Obsolete($"Use {nameof(ChartFile.ReadInstrumentsAsync)} with a component list and {nameof(Metadata.Formatting)}.")]
    public static async Task<DirectoryResult<Track<DrumsChord>?>> FromDirectoryAsync(string directory, Difficulty difficulty, ReadingConfiguration? config = default, CancellationToken cancellationToken = default) => await DirectoryHandler.FromDirectoryAsync(directory, async (path, formatting) => await FromFileAsync(path, difficulty, config, formatting, cancellationToken), cancellationToken);

    [Obsolete($"Use {nameof(ChartFile.ReadInstruments)} with a component list and {nameof(Metadata.Formatting)}.")]
    public static DirectoryResult<Track<GHLChord>?> FromDirectory(string directory, GHLInstrumentIdentity instrument, Difficulty difficulty, ReadingConfiguration? config = default) => DirectoryHandler.FromDirectory(directory, (path, formatting) => FromFile(path, instrument, difficulty, config, formatting));

    [Obsolete($"Use {nameof(ChartFile.ReadInstrumentsAsync)} with a component list and {nameof(Metadata.Formatting)}.")]
    public static async Task<DirectoryResult<Track<GHLChord>?>> FromDirectoryAsync(string directory, GHLInstrumentIdentity instrument, Difficulty difficulty, ReadingConfiguration? config = default, CancellationToken cancellationToken = default) => await DirectoryHandler.FromDirectoryAsync(directory, async (path, formatting) => await FromFileAsync(path, instrument, difficulty, config, formatting, cancellationToken), cancellationToken);

    [Obsolete($"Use {nameof(ChartFile.ReadInstruments)} with a component list and {nameof(Metadata.Formatting)}.")]
    public static DirectoryResult<Track<StandardChord>?> FromDirectory(string directory, StandardInstrumentIdentity instrument, Difficulty difficulty, ReadingConfiguration? config = default) => DirectoryHandler.FromDirectory(directory, (path, formatting) => FromFile(path, instrument, difficulty, config, formatting));

    [Obsolete($"Use {nameof(ChartFile.ReadInstrumentsAsync)} with a component list and {nameof(Metadata.Formatting)}.")]
    public static async Task<DirectoryResult<Track<StandardChord>?>> FromDirectoryAsync(string directory, StandardInstrumentIdentity instrument, Difficulty difficulty, ReadingConfiguration? config = default, CancellationToken cancellationToken = default) => await DirectoryHandler.FromDirectoryAsync(directory, async (path, formatting) => await FromFileAsync(path, instrument, difficulty, config, formatting, cancellationToken), cancellationToken);
    #endregion
    #endregion

    [Obsolete($"Use {nameof(ChartFile.ReplaceInstruments)} with a component list.")]
    public void ToFile(string path, WritingConfiguration? config = default, FormattingRules? formatting = default) => ExtensionHandler.Write(path, this, (".chart", (path, track) => ChartFile.ReplaceTrack(path, track, config?.Chart, formatting)));

    [Obsolete($"Use {nameof(ChartFile.ReplaceInstrumentsAsync)} with a component list.")]
    public async Task ToFileAsync(string path, WritingConfiguration? config = default,FormattingRules? formatting = default, CancellationToken cancellationToken = default) => await ExtensionHandler.WriteAsync(path, this, (".chart", (path, track) => ChartFile.ReplaceTrackAsync(path, track, config?.Chart, formatting, cancellationToken)));
}
