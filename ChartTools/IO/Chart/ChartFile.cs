﻿using ChartTools.Events;
using ChartTools.Extensions;
using ChartTools.Extensions.Linq;
using ChartTools.IO.Chart.Configuration;
using ChartTools.IO.Chart.Configuration.Sessions;
using ChartTools.IO.Chart.Parsing;
using ChartTools.IO.Chart.Serializing;
using ChartTools.IO.Configuration;
using ChartTools.IO.Formatting;
using ChartTools.Lyrics;

namespace ChartTools.IO.Chart;

/// <summary>
/// Provides methods for reading and writing chart files
/// </summary>
public static class ChartFile
{
    /// <summary>
    /// Default configuration to use for reading when the provided configuration is <see langword="default"/>
    /// </summary>
    public static ChartReadingConfiguration DefaultReadConfig { get; set; } = new()
    {
        DuplicateTrackObjectPolicy = DuplicateTrackObjectPolicy.ThrowException,
        OverlappingStarPowerPolicy = OverlappingSpecialPhrasePolicy.ThrowException,
        SnappedNotesPolicy         = SnappedNotesPolicy.ThrowException,
        SoloNoStarPowerPolicy      = SoloNoStarPowerPolicy.Convert,
        TempolessAnchorPolicy      = TempolessAnchorPolicy.ThrowException,
        UnknownSectionPolicy       = UnknownSectionPolicy.ThrowException
    };

    /// <summary>
    /// Default configuration to use for writing when the provided configuration is <see langword="default"/>
    /// </summary>
    public static ChartWritingConfiguration DefaultWriteConfig { get; set; } = new()
    {
        DuplicateTrackObjectPolicy = DuplicateTrackObjectPolicy.ThrowException,
        OverlappingStarPowerPolicy = OverlappingSpecialPhrasePolicy.ThrowException,
        SoloNoStarPowerPolicy      = SoloNoStarPowerPolicy.Convert,
        SnappedNotesPolicy         = SnappedNotesPolicy.ThrowException,
        UnsupportedModifierPolicy  = UnsupportedModifierPolicy.ThrowException
    };

    #region Reading
    #region Song
    /// <summary>
    /// Combines the results from the parsers of a <see cref="ChartFileReader"/> into a <see cref="Song"/>.
    /// </summary>
    /// <param name="reader">Reader to get the parsers from</param>
    private static Song CreateSongFromReader(ChartFileReader reader)
    {
        Song song = new();

        foreach (var parser in reader.Parsers)
            parser.ApplyToSong(song);

        return song;
    }

    /// <inheritdoc cref="Song.FromFile(string, ReadingConfiguration?, FormattingRules?)"/>
    /// <param name="path"><inheritdoc cref="Song.FromFile(string, ReadingConfiguration?, FormattingRules?)" path="/param[@name='path']"/></param>
    /// <param name="config"><inheritdoc cref="Song.FromFile(string, ReadingConfiguration?, FormattingRules?)" path="/param[@name='config']"/></param>
    public static Song ReadSong(string path, ChartReadingConfiguration? config = default, FormattingRules? formatting = default)
    {
        var session = new ChartReadingSession(ComponentList.Full, config, formatting);
        var reader = new ChartFileReader(path, session);

        reader.Read();
        return CreateSongFromReader(reader);
    }

    public static async Task<Song> ReadSongAsync(string path, ChartReadingConfiguration? config = default, FormattingRules? formatting = default, CancellationToken cancellationToken = default)
    {
        var session = new ChartReadingSession(ComponentList.Full, config, formatting);
        var reader = new ChartFileReader(path, session);

        await reader.ReadAsync(cancellationToken);
        return CreateSongFromReader(reader);
    }

    public static Song ReadComponents(string path, ComponentList components, ChartReadingConfiguration? config = default, FormattingRules? formatting = default)
    {

        var session = new ChartReadingSession(components, config, formatting);
        var reader = new ChartFileReader(path, session);

        reader.Read();
        return CreateSongFromReader(reader);
    }

    public static async Task<Song> ReadComponents(string path, ComponentList components, ChartReadingConfiguration? config = default, FormattingRules? formatting = default, CancellationToken cancellationToken = default)
    {

        var session = new ChartReadingSession(components, config, formatting);
        var reader = new ChartFileReader(path, session);

        await reader.ReadAsync(cancellationToken);
        return CreateSongFromReader(reader);
    }
    #endregion

    #region Instruments
    /// <summary>
    /// Combines the results from the parsers in a <see cref="ChartFileReader"/> into an instrument.
    /// </summary>
    private static TInst? CreateInstrumentFromReader<TInst, TChord>(ChartFileReader reader) where TInst : Instrument<TChord>, new() where TChord : IChord, new()
    {
        TInst? output = null;

        foreach (var parser in reader.Parsers)
            (output ??= new()).SetTrack(((TrackParser<TChord>)parser).Result!);

        return output;
    }

    /// <summary>
    /// Reads an instrument from a chart file.
    /// </summary>
    /// <returns>Instance of <see cref="Instrument"/> containing all data about the given instrument
    ///     <para><see langword="null"/> if the file contains no data for the given instrument</para>
    /// </returns>
    /// <param name="path">Path of the file to read</param>
    /// <param name="instrument">Instrument to read</param>
    /// <param name="config"><inheritdoc cref="ReadingConfiguration" path="/summary"/></param>
    public static Instrument? ReadInstrument(string path, InstrumentIdentity instrument, ChartReadingConfiguration? config = default, FormattingRules? formatting = default)
    {
        if (instrument == InstrumentIdentity.Drums)
            return ReadDrums(path, config, formatting);
        if (Enum.IsDefined((GHLInstrumentIdentity)instrument))
            return ReadInstrument(path, (GHLInstrumentIdentity)instrument, config, formatting);
        return Enum.IsDefined((StandardInstrumentIdentity)instrument)
            ? ReadInstrument(path, (StandardInstrumentIdentity)instrument, config, formatting)
            : throw new UndefinedEnumException(instrument);
    }
    public static async Task<Instrument?> ReadInstrumentAsync(string path, InstrumentIdentity instrument, ChartReadingConfiguration? config = default, FormattingRules? formatting = default, CancellationToken cancellationToken = default)
    {
        if (instrument == InstrumentIdentity.Drums)
            return await ReadDrumsAsync(path, config, formatting, cancellationToken);
        if (Enum.IsDefined((GHLInstrumentIdentity)instrument))
            return await ReadInstrumentAsync(path, (GHLInstrumentIdentity)instrument, config, formatting, cancellationToken);
        return Enum.IsDefined((StandardInstrumentIdentity)instrument)
            ? await ReadInstrumentAsync(path, (StandardInstrumentIdentity)instrument, config, formatting, cancellationToken)
            : throw new UndefinedEnumException(instrument);
    }

    #region Vocals
    /// <summary>
    /// Reads vocals from the global events in a chart file.
    /// </summary>
    /// <returns>Instance of <see cref="Instrument{TChord}"/> where TChord is <see cref="Phrase"/> containing lyric and timing data
    ///     <para><see langword="null"/> if the file contains no drums data</para>
    /// </returns>
    /// <param name="path">Path of the file to read</param>
    public static Vocals? ReadVocals(string path) => BuildVocals(ReadGlobalEvents(path));
    public static async Task<Vocals?> ReadVocalsAsync(string path, CancellationToken cancellationToken = default) => BuildVocals(await ReadGlobalEventsAsync(path, cancellationToken));

    private static Vocals? BuildVocals(IList<GlobalEvent> events)
    {
        var lyrics = events.GetLyrics().ToArray();

        if (lyrics.Length == 0)
            return null;

        var instument = new Vocals();

        foreach (var diff in EnumCache<Difficulty>.Values)
        {
            var track = instument.CreateTrack(diff);
            track.Chords.AddRange(lyrics);
        }

        return instument;
    }
    #endregion

    #region Drums
    /// <summary>
    /// Reads drums from a chart file.
    /// </summary>
    /// <returns>Instance of <see cref="Instrument{TChord}"/> where TChord is <see cref="DrumsChord"/> containing all drums data
    ///     <para><see langword="null"/> if the file contains no drums data</para>
    /// </returns>
    /// <param name="path">Path of the file to read</param>
    /// <param name="config"><inheritdoc cref="ReadingConfiguration" path="/summary"/></param>
    /// <param name="formatting"><inheritdoc cref="FormattingRules" path="/summary"/></param>
    public static Drums? ReadDrums(string path, ChartReadingConfiguration? config = default, FormattingRules? formatting = default)
    {
        var session = new ChartReadingSession(new(InstrumentIdentity.Drums), config, formatting);
        var reader = new ChartFileReader(path, session);

        reader.Read();
        return CreateInstrumentFromReader<Drums, DrumsChord>(reader);
    }
    public static async Task<Drums?> ReadDrumsAsync(string path, ChartReadingConfiguration? config = default, FormattingRules? formatting = default, CancellationToken cancellationToken = default)
    {
        var session = new ChartReadingSession(new(InstrumentIdentity.Drums), config, formatting);
        var reader = new ChartFileReader(path, session);

        await reader.ReadAsync(cancellationToken);
        return CreateInstrumentFromReader<Drums, DrumsChord>(reader);
    }
    #endregion

    #region GHL
    /// <summary>
    /// Reads a Guitar Hero Live instrument from a chart file.
    /// </summary>
    /// <returns>Instance of <see cref="Instrument{TChord}"/> where TChord is <see cref="GHLChord"/> containing all data about the given instrument
    ///     <para><see langword="null"/> if the file has no data for the given instrument</para>
    /// </returns>
    /// <param name="path">Path of the file to read</param>
    public static GHLInstrument? ReadInstrument(string path, GHLInstrumentIdentity instrument, ChartReadingConfiguration? config = default, FormattingRules? formatting = default)
    {
        var session = new ChartReadingSession(new(instrument), config, formatting);
        var reader  = new ChartFileReader(path, session);

        reader.Read();
        return CreateInstrumentFromReader<GHLInstrument, GHLChord>(reader);
    }
    public static async Task<GHLInstrument?> ReadInstrumentAsync(string path, GHLInstrumentIdentity instrument, ChartReadingConfiguration? config = default, FormattingRules? formatting = default, CancellationToken cancellationToken = default)
    {
        var session = new ChartReadingSession(new(instrument), config, formatting);
        var reader = new ChartFileReader(path, session);

        await reader.ReadAsync(cancellationToken);
        return CreateInstrumentFromReader<GHLInstrument, GHLChord>(reader);
    }
    #endregion

    #region Standard
    public static StandardInstrument? ReadInstrument(string path, StandardInstrumentIdentity instrument, ChartReadingConfiguration? config = default, FormattingRules? formatting = default)
    {
        var session = new ChartReadingSession(new(instrument), config, formatting);
        var reader = new ChartFileReader(path, session);

        reader.Read();
        return CreateInstrumentFromReader<StandardInstrument, StandardChord>(reader);
    }

    public static async Task<StandardInstrument?> ReadInstrumentAsync(string path, StandardInstrumentIdentity instrument, ChartReadingConfiguration? config = default, FormattingRules? formatting = default, CancellationToken cancellationToken = default)
    {
        var session = new ChartReadingSession(new(instrument), config, formatting);
        var reader = new ChartFileReader(path, session);

        await reader.ReadAsync(cancellationToken);
        return CreateInstrumentFromReader<StandardInstrument, StandardChord>(reader);
    }
    #endregion
    #endregion

    #region Tracks
    /// <inheritdoc cref="Track.FromFile(string, InstrumentIdentity, Difficulty, ReadingConfiguration?, FormattingRules?)"/>
    /// <param name="path"><inheritdoc cref="Track.FromFile(string, InstrumentIdentity, Difficulty, ReadingConfiguration?, FormattingRules?)" path="/param[@name='path']"/></param>
    /// <param name="instrument"><inheritdoc cref="Track.FromFile(string, InstrumentIdentity, Difficulty, ReadingConfiguration?, FormattingRules?)" path="/param[@name='instrument']"/></param>
    /// <param name="difficulty"><inheritdoc cref="Track.FromFile(string, InstrumentIdentity, Difficulty, ReadingConfiguration?, FormattingRules?)" path="/param[@name='difficulty']"/></param>
    /// <param name="config"><inheritdoc cref="Track.FromFile(string, InstrumentIdentity, Difficulty, ReadingConfiguration?, FormattingRules?)" path="/param[@name='config']"/></param>
    public static Track ReadTrack(string path, InstrumentIdentity instrument, Difficulty difficulty, ChartReadingConfiguration? config = default, FormattingRules? formatting = default)
    {
        if (instrument == InstrumentIdentity.Drums)
            return ReadDrumsTrack(path, difficulty, config, formatting);
        if (Enum.IsDefined((GHLInstrumentIdentity)instrument))
            return ReadTrack(path, (GHLInstrumentIdentity)instrument, difficulty, config, formatting);
        if (Enum.IsDefined((StandardInstrumentIdentity)instrument))
            return ReadTrack(path, (StandardInstrumentIdentity)instrument, difficulty, config, formatting);

        throw new UndefinedEnumException(instrument);
    }
    /// <inheritdoc cref="Track.FromFileAsync(string, InstrumentIdentity, Difficulty, ReadingConfiguration?, FormattingRules?, CancellationToken)"/>
    /// <param name="path"><inheritdoc cref="Track.FromFileAsync(string, InstrumentIdentity, Difficulty, ReadingConfiguration?, FormattingRules?, CancellationToken)" path="/param[@name='path']"/></param>
    /// <param name="instrument"><inheritdoc cref="Track.FromFileAsync(string, InstrumentIdentity, Difficulty, ReadingConfiguration?, FormattingRules?, CancellationToken)" path="/param[@name='instrument']"/></param>
    /// <param name="difficulty"><inheritdoc cref="Track.FromFileAsync(string, InstrumentIdentity, Difficulty, ReadingConfiguration?, FormattingRules?, CancellationToken)" path="/param[@name='difficulty']"/></param>
    /// <param name="cancellationToken"><inheritdoc cref="Track.FromFileAsync(string, InstrumentIdentity, Difficulty, ReadingConfiguration?, FormattingRules?, CancellationToken)" path="/param[@name='cancellationToken']"/></param>
    /// <param name="config"><inheritdoc cref="Track.FromFileAsync(string, InstrumentIdentity, Difficulty, ReadingConfiguration?, FormattingRules?, CancellationToken)" path="/param[@name='config']"/></param>
    public static async Task<Track> ReadTrackAsync(string path, InstrumentIdentity instrument, Difficulty difficulty, ChartReadingConfiguration? config = default, FormattingRules? formatting = default, CancellationToken cancellationToken = default)
    {
        if (instrument == InstrumentIdentity.Drums)
            return await ReadDrumsTrackAsync(path, difficulty, config, formatting, cancellationToken);
        if (Enum.IsDefined((GHLInstrumentIdentity)instrument))
            return await ReadTrackAsync(path, (GHLInstrumentIdentity)instrument, difficulty, config, formatting, cancellationToken);
        if (Enum.IsDefined((StandardInstrumentIdentity)instrument))
            return await ReadTrackAsync(path, (StandardInstrumentIdentity)instrument, difficulty, config, formatting, cancellationToken);

        throw new UndefinedEnumException(instrument);
    }
    #region Drums
    /// <inheritdoc cref="Track.FromFile(string, Difficulty, ReadingConfiguration?, FormattingRules?)"/>
    /// <param name="path"><inheritdoc cref="Track.FromFile(string, Difficulty, ReadingConfiguration?, FormattingRules?)" path="/param[@name='path']"/></param>
    /// <param name="difficulty"><inheritdoc cref="Track.FromFile(string, Difficulty, ReadingConfiguration?, FormattingRules?)" path="/param[@name='difficulty']"/></param>
    /// <param name="config"><inheritdoc cref="Track.FromFile(string, Difficulty, ReadingConfiguration?, FormattingRules?)" path="/param[@name='config']"/></param>
    public static Track<DrumsChord> ReadDrumsTrack(string path, Difficulty difficulty, ChartReadingConfiguration? config = default, FormattingRules? formatting = default)
    {
        var session = new ChartReadingSession(new(InstrumentIdentity.Drums, difficulty), config, formatting);
        var reader = new ChartFileReader(path, session);

        reader.Read();
        return reader.Parsers.TryGetFirstOfType(out DrumsTrackParser? parser) ? parser!.Result! : new();
    }

    /// <inheritdoc cref="Track.FromFileAsync(string, Difficulty, ReadingConfiguration?, FormattingRules?, CancellationToken)"/>
    /// <param name="path"><inheritdoc cref="Track.FromFileAsync(string, Difficulty, ReadingConfiguration?, FormattingRules?, CancellationToken)" path="/param[@name='path']"/></param>
    /// <param name="difficulty"><inheritdoc cref="Track.FromFileAsync(string, Difficulty, ReadingConfiguration?, FormattingRules?, CancellationToken)" path="/param[@name='difficulty']"/></param>
    /// <param name="cancellationToken"><inheritdoc cref="Track.FromFileAsync(string, Difficulty, ReadingConfiguration?, FormattingRules?, CancellationToken)" path="/param[@name='cancellationToken']"/></param>
    /// <param name="config"><inheritdoc cref="Track.FromFileAsync(string, Difficulty, ReadingConfiguration?, FormattingRules?, CancellationToken)" path="/param[@name='config']"/></param>
    public static async Task<Track<DrumsChord>> ReadDrumsTrackAsync(string path, Difficulty difficulty, ChartReadingConfiguration? config = default, FormattingRules? formatting = default, CancellationToken cancellationToken = default)
    {
        var session = new ChartReadingSession(new(InstrumentIdentity.Drums, difficulty), config, formatting);
        var reader = new ChartFileReader(path, session);

        await reader.ReadAsync(cancellationToken);
        return reader.Parsers.TryGetFirstOfType(out DrumsTrackParser? parser) ? parser!.Result! : new();
    }
    #endregion

    #region GHL

    /// <inheritdoc cref="Track.FromFileAsync(string, GHLInstrumentIdentity, Difficulty, ReadingConfiguration?, FormattingRules?, CancellationToken)"/>
    /// <param name="path"><inheritdoc cref="Track.FromFileAsync(string, GHLInstrumentIdentity, Difficulty, ReadingConfiguration?, FormattingRules?, CancellationToken)" path="/param[@name='path']"/></param>
    /// <param name="instrument"><inheritdoc cref="Track.FromFileAsync(string, GHLInstrumentIdentity, Difficulty, ReadingConfiguration?, FormattingRules?, CancellationToken)" path="/param[@name='instrument']"/></param>
    /// <param name="difficulty"><inheritdoc cref="Track.FromFileAsync(string, GHLInstrumentIdentity, Difficulty, ReadingConfiguration?, FormattingRules?, CancellationToken)" path="/param[@name='difficulty']"/></param>
    /// <param name="config"><inheritdoc cref="Track.FromFileAsync(string, GHLInstrumentIdentity, Difficulty, ReadingConfiguration?, FormattingRules?, CancellationToken)" path="/param[@name='config']"/></param>
    public static Track<GHLChord> ReadTrack(string path, GHLInstrumentIdentity instrument, Difficulty difficulty, ChartReadingConfiguration? config = default, FormattingRules? formatting = default)
    {
        var session = new ChartReadingSession(new(instrument, difficulty), config, formatting);
        var reader = new ChartFileReader(path, session);

        reader.Read();
        return reader.Parsers.TryGetFirstOfType(out GHLTrackParser? parser) ? parser!.Result : new();
    }

    /// <inheritdoc cref="Track.FromFileAsync(string, GHLInstrumentIdentity, Difficulty, ReadingConfiguration?, FormattingRules?, CancellationToken)"/>
    /// <param name="path"><inheritdoc cref="Track.FromFileAsync(string, GHLInstrumentIdentity, Difficulty, ReadingConfiguration?, FormattingRules?, CancellationToken)" path="/param[@name='path']"/></param>
    /// <param name="instrument"><inheritdoc cref="Track.FromFileAsync(string, GHLInstrumentIdentity, Difficulty, ReadingConfiguration?, FormattingRules?, CancellationToken)" path="/param[@name='instrument']"/></param>
    /// <param name="difficulty"><inheritdoc cref="Track.FromFileAsync(string, GHLInstrumentIdentity, Difficulty, ReadingConfiguration?, FormattingRules?, CancellationToken)" path="/param[@name='difficulty']"/></param>
    /// <param name="cancellationToken"><inheritdoc cref="Track.FromFileAsync(string, GHLInstrumentIdentity, Difficulty, ReadingConfiguration?, FormattingRules?, CancellationToken)" path="/param[@name='cancellationToken']"/></param>
    /// <param name="config"><inheritdoc cref="Track.FromFileAsync(string, GHLInstrumentIdentity, Difficulty, ReadingConfiguration?, FormattingRules?, CancellationToken)" path="/param[@name='config']"/></param>
    public static async Task<Track<GHLChord>> ReadTrackAsync(string path, GHLInstrumentIdentity instrument, Difficulty difficulty, ChartReadingConfiguration? config, FormattingRules? formatting = default, CancellationToken cancellationToken = default)
    {
        var session = new ChartReadingSession(new(instrument, difficulty), config, formatting);
        var reader = new ChartFileReader(path, session);

        await reader.ReadAsync(cancellationToken);
        return reader.Parsers.TryGetFirstOfType(out GHLTrackParser? parser) ? parser!.Result : new();
    }
    #endregion

    #region Standard
    /// <inheritdoc cref="Track.FromFile(string, StandardInstrumentIdentity, Difficulty, ReadingConfiguration?, FormattingRules?)"/>
    /// <param name="path"><inheritdoc cref="Track.FromFile(string, StandardInstrumentIdentity, Difficulty, ReadingConfiguration?, FormattingRules?)" path="/param[@name='path']"/></param>
    /// <param name="instrument"><inheritdoc cref="Track.FromFile(string, StandardInstrumentIdentity, Difficulty, ReadingConfiguration?, FormattingRules?)" path="/param[@name='instrument']"/></param>
    /// <param name="difficulty"><inheritdoc cref="Track.FromFile(string, StandardInstrumentIdentity, Difficulty, ReadingConfiguration?, FormattingRules?)" path="/param[@name='difficulty']"/></param>
    /// <param name="config"><inheritdoc cref="Track.FromFile(string, StandardInstrumentIdentity, Difficulty, ReadingConfiguration?, FormattingRules?)" path="/param[@name='config']"/></param>
    public static Track<StandardChord> ReadTrack(string path, StandardInstrumentIdentity instrument, Difficulty difficulty, ChartReadingConfiguration? config = default, FormattingRules? formatting = default)
    {
        var session = new ChartReadingSession(new(instrument, difficulty), config, formatting);
        var reader = new ChartFileReader(path, session);

        reader.Read();
        return reader.Parsers.TryGetFirstOfType(out StandardTrackParser? parser) ? parser!.Result! : new();
    }

    /// <inheritdoc cref="Track.FromFileAsync(string, StandardInstrumentIdentity, Difficulty, ReadingConfiguration?, FormattingRules?, CancellationToken)"/>
    /// <param name="path"><inheritdoc cref="Track.FromFileAsync(string, StandardInstrumentIdentity, Difficulty, ReadingConfiguration?, FormattingRules?, CancellationToken)" path="/param[@name='path']"/></param>
    /// <param name="instrument"><inheritdoc cref="Track.FromFileAsync(string, StandardInstrumentIdentity, Difficulty, ReadingConfiguration?, FormattingRules?, CancellationToken)" path="/param[@name='instrument']"/></param>
    /// <param name="difficulty"><inheritdoc cref="Track.FromFileAsync(string, StandardInstrumentIdentity, Difficulty, ReadingConfiguration?, FormattingRules?, CancellationToken)" path="/param[@name='difficulty']"/></param>
    /// <param name="cancellationToken"><inheritdoc cref="Track.FromFileAsync(string, StandardInstrumentIdentity, Difficulty, ReadingConfiguration?, FormattingRules?, CancellationToken)" path="/param[@name='cancellationToken']"/></param>
    /// <param name="config"><inheritdoc cref="Track.FromFileAsync(string, StandardInstrumentIdentity, Difficulty, ReadingConfiguration?, FormattingRules?, CancellationToken)" path="/param[@name='config']"/></param>
    /// <returns></returns>
    public static async Task<Track<StandardChord>> ReadTrackAsync(string path, StandardInstrumentIdentity instrument, Difficulty difficulty, ChartReadingConfiguration? config = default, FormattingRules? formatting = default, CancellationToken cancellationToken = default)
    {
        var session = new ChartReadingSession(new(instrument, difficulty), config, formatting);
        var reader = new ChartFileReader(path, session);

        await reader.ReadAsync(cancellationToken);
        return reader.Parsers.TryGetFirstOfType(out StandardTrackParser? parser) ? parser!.Result! : new();
    }
    #endregion
    #endregion

    #region Metadata
    /// <summary>
    /// Reads metadata from a chart file.
    /// </summary>
    /// <param name="path">Path of the file to read</param>
    public static Metadata ReadMetadata(string path)
    {
        var session = new ChartReadingSession(new() { Metadata = true }, DefaultReadConfig, null);
        var reader = new ChartFileReader(path, session);

        reader.Read();
        return reader.Parsers.TryGetFirstOfType(out MetadataParser? parser) ? parser!.Result : new();
    }
    #endregion

    #region Global events
    /// <inheritdoc cref="GlobalEvent.FromFile(string)"/>
    /// <param name="path"><inheritdoc cref="GlobalEvent.FromFile(string)" path="/param[@name='path']"/></param>
    public static List<GlobalEvent> ReadGlobalEvents(string path)
    {
        var session = new ChartReadingSession(new() { GlobalEvents = true }, DefaultReadConfig, null);
        var reader = new ChartFileReader(path, session);

        reader.Read();
        return reader.Parsers.TryGetFirstOfType(out GlobalEventParser? parser) ? parser!.Result! : [];
    }

    /// <inheritdoc cref="GlobalEvent.FromFileAsync(string, CancellationToken)"/>
    /// <param name="path"><inheritdoc cref="GlobalEvent.FromFileAsync(string, CancellationToken)" path="/param[@name='path']"/></param>
    /// <param name="cancellationToken"><inheritdoc cref="GlobalEvent.FromFileAsync(string, CancellationToken)" path="/param[@name='cancellationToken']"/></param>
    /// <returns></returns>
    public static async Task<List<GlobalEvent>> ReadGlobalEventsAsync(string path, CancellationToken cancellationToken = default)
    {
        var session = new ChartReadingSession(new() { GlobalEvents = true }, DefaultReadConfig, null);
        var reader = new ChartFileReader(path, session);

        await reader.ReadAsync(cancellationToken);
        return reader.Parsers.TryGetFirstOfType(out GlobalEventParser? parser) ? parser!.Result! : [];
    }

    /// <summary>
    /// Reads lyrics from a chart file.
    /// </summary>
    /// <returns>Enumerable of <see cref="Phrase"/> containing the lyrics from the file</returns>
    /// <param name="path">Path of the file to read</param>
    /// <inheritdoc cref="ReadGlobalEvents(string)" path="/exception"/>
    public static IEnumerable<Phrase> ReadLyrics(string path) => ReadGlobalEvents(path).GetLyrics();
    /// <summary>
    /// Reads lyrics from a chart file asynchronously using multitasking.
    /// </summary>
    /// <param name="path"><inheritdoc cref="ReadLyrics(string)" path="/param[@name='path']"/></param>
    /// <param name="cancellationToken">Token to request cancellation</param>
    public static async Task<IEnumerable<Phrase>> ReadLyricsAsync(string path, CancellationToken cancellationToken = default) => (await ReadGlobalEventsAsync(path, cancellationToken)).GetLyrics();
    #endregion

    #region Sync track

    /// <inheritdoc cref="SyncTrack.FromFile(string, ReadingConfiguration?)"/>
    /// <param name="path"><inheritdoc cref="SyncTrack.FromFile(string, ReadingConfiguration?)" path="/param[@name='path']"/></param>
    /// <param name="config"><inheritdoc cref="SyncTrack.FromFile(string, ReadingConfiguration?)" path="/param[@name='config']"/></param>
    public static SyncTrack ReadSyncTrack(string path, ChartReadingConfiguration? config = default, FormattingRules? formatting = default)
    {
        var session = new ChartReadingSession(new() { SyncTrack = true }, config, null);
        var reader = new ChartFileReader(path, session);

        reader.Read();
        return reader.Parsers.TryGetFirstOfType(out SyncTrackParser? syncTrackParser) ? syncTrackParser!.Result! : new();
    }

    /// <inheritdoc cref="SyncTrack.FromFileAsync(string, ReadingConfiguration?, CancellationToken)"/>
    /// <param name="path"><inheritdoc cref="SyncTrack.FromFileAsync(string, ReadingConfiguration?, CancellationToken)" path="/param[­@name='path']"/></param>
    /// <param name="cancellationToken"><inheritdoc cref="SyncTrack.FromFileAsync(string, ReadingConfiguration?, CancellationToken)" path="/param[­@name='cancellationToken']"/></param>
    /// <param name="config"><inheritdoc cref="SyncTrack.FromFileAsync(string, ReadingConfiguration?, CancellationToken)" path="/param[­@name='config']"/></param>
    public static async Task<SyncTrack> ReadSyncTrackAsync(string path, ChartReadingConfiguration? config = default, CancellationToken cancellationToken = default)
    {
        var session = new ChartReadingSession(new() { SyncTrack = true }, config, null);
        var reader = new ChartFileReader(path, session);

        await reader.ReadAsync(cancellationToken);
        return reader.Parsers.TryGetFirstOfType(out SyncTrackParser? syncTrackParser) ? syncTrackParser!.Result! : new();
    }
    #endregion
    #endregion

    #region Writing
    /// <summary>
    /// Writes a song to a chart file.
    /// </summary>
    /// <param name="path">Path of the file to write</param>
    /// <param name="song">Song to write</param>
    public static void WriteSong(string path, Song song, ChartWritingConfiguration? config = default)
    {
        var writer = GetSongWriter(path, song, new(config, song.Metadata.Formatting));
        writer.Write();
    }
    public static async Task WriteSongAsync(string path, Song song, ChartWritingConfiguration? config = default, CancellationToken cancellationToken = default)
    {
        var writer = GetSongWriter(path, song, new(config, song.Metadata.Formatting));
        await writer.WriteAsync(cancellationToken);
    }
    private static ChartFileWriter GetSongWriter(string path, Song song, ChartWritingSession session)
    {
        var instruments = song.Instruments.NonNull().ToArray();
        var serializers = new List<Serializer<string>>(instruments.Length + 2);
        var removedHeaders = new List<string>();

        serializers.Add(new MetadataSerializer(song.Metadata));

        if (!song.SyncTrack.IsEmpty)
            serializers.Add(new SyncTrackSerializer(song.SyncTrack, session));
        else
            removedHeaders.Add(ChartFormatting.SyncTrackHeader);

        if (song.GlobalEvents.Count > 0)
            serializers.Add(new GlobalEventSerializer(song.GlobalEvents, session));
        else
            removedHeaders.Add(ChartFormatting.GlobalEventHeader);

        var difficulties = EnumCache<Difficulty>.Values;

        // Remove headers for null instruments
        removedHeaders.AddRange((from identity in Enum.GetValues<InstrumentIdentity>()
                                 where instruments.Any(instrument => instrument.InstrumentIdentity == identity)
                                 let instrumentName = ChartFormatting.InstrumentHeaderNames[identity]
                                 let headers = from diff in difficulties
                                               select ChartFormatting.Header(identity, diff)
                                 select headers).SelectMany(h => h));

        foreach (var instrument in instruments)
        {
            var instrumentName = ChartFormatting.InstrumentHeaderNames[instrument.InstrumentIdentity];
            var tracks = instrument.GetExistingTracks().ToArray();

            serializers.AddRange(tracks.Select(t => new TrackSerializer(t, session)));
            removedHeaders.AddRange(difficulties.Where(diff => !tracks.Any(t => t.Difficulty == diff)).Select(diff => ChartFormatting.Header(instrumentName, diff)));
        }

        if (song.UnknownChartSections is not null)
            serializers.AddRange(song.UnknownChartSections.Select(s => new UnknownSectionSerializer(s.Header, s, session)));

        return new(path, removedHeaders, serializers.ToArray());
    }

    /// <summary>
    /// Replaces an instrument in a file.
    /// </summary>
    /// <param name="path">Path of the file to write</param>
    public static void ReplaceInstrument(string path, Instrument instrument, ChartWritingConfiguration? config = default, FormattingRules? formatting = default)
    {
        var writer = GetInstrumentWriter(path, instrument, new(config, formatting ?? new()));
        writer.Write();
    }
    public static async Task ReplaceInstrumentAsync(string path, Instrument instrument, ChartWritingConfiguration? config = default, FormattingRules? formatting = default, CancellationToken cancellationToken = default)
    {
        var writer = GetInstrumentWriter(path, instrument, new(config, formatting ?? new()));
        await writer.WriteAsync(cancellationToken);
    }
    private static ChartFileWriter GetInstrumentWriter(string path, Instrument instrument, ChartWritingSession session)
    {
        if (!Enum.IsDefined(instrument.InstrumentIdentity))
            throw new ArgumentException("Instrument cannot be written because its identity is unknown.", nameof(instrument));

        var instrumentName = ChartFormatting.InstrumentHeaderNames[instrument.InstrumentIdentity];
        var tracks = instrument.GetExistingTracks().ToArray();

        return new(path,
            EnumCache<Difficulty>.Values.Where(d => !tracks.Any(t => t.Difficulty == d)).Select(d => ChartFormatting.Header(instrumentName, d)),
            tracks.Select(t => new TrackSerializer(t, session)).ToArray());
    }

    public static void ReplaceTrack(string path, Track track, ChartWritingConfiguration? config = default, FormattingRules? formatting = default)
    {
        var writer = GetTrackWriter(path, track, new(config, formatting ?? new()));
        writer.Write();
    }
    public static async Task ReplaceTrackAsync(string path, Track track, ChartWritingConfiguration? config = default, FormattingRules? formatting = default, CancellationToken cancellationToken = default)
    {
        var writer = GetTrackWriter(path, track, new(config, formatting ?? new()));
        await writer.WriteAsync(cancellationToken);
    }
    private static ChartFileWriter GetTrackWriter(string path, Track track, ChartWritingSession session)
    {
        if (track.ParentInstrument is null)
            throw new ArgumentNullException(nameof(track), "Cannot write track because it does not belong to an instrument.");
        if (!Enum.IsDefined(track.ParentInstrument.InstrumentIdentity))
            throw new ArgumentException("Cannot write track because the instrument it belongs to is unknown.", nameof(track));

        return new(path, null, new TrackSerializer(track, session));
    }

    /// <summary>
    /// Replaces the metadata in a file.
    /// </summary>
    /// <param name="path">Path of the file to read</param>
    /// <param name="metadata">Metadata to write</param>
    public static void ReplaceMetadata(string path, Metadata metadata)
    {
        var writer = GetMetadataWriter(path, metadata);
        writer.Write();
    }
    private static ChartFileWriter GetMetadataWriter(string path, Metadata metadata) => new(path, null, new MetadataSerializer(metadata));

    /// <summary>
    /// Replaces the global events in a file.
    /// </summary>
    /// <param name="path">Path of the file to write</param>
    /// <param name="events">Events to use as a replacement</param>
    public static void ReplaceGlobalEvents(string path, IEnumerable<GlobalEvent> events)
    {
        var writer = GetGlobalEventWriter(path, events, new(DefaultWriteConfig, null));
        writer.Write();
    }
    public static async Task ReplaceGlobalEventsAsync(string path, IEnumerable<GlobalEvent> events, CancellationToken cancellationToken = default)
    {
        var writer = GetGlobalEventWriter(path, events, new(DefaultWriteConfig, null));
        await writer.WriteAsync(cancellationToken);
    }
    private static ChartFileWriter GetGlobalEventWriter(string path, IEnumerable<GlobalEvent> events, ChartWritingSession session) => new(path, null, new GlobalEventSerializer(events, session));

    /// <summary>
    /// Replaces the sync track in a file.
    /// </summary>
    /// <param name="path">Path of the file to write</param>
    /// <param name="syncTrack">Sync track to write</param>
    /// <param name="config"><inheritdoc cref="ReadingConfiguration" path="/summary"/></param>
    public static void ReplaceSyncTrack(string path, SyncTrack syncTrack, ChartWritingConfiguration? config = default)
    {
        var writer = GetSyncTrackWriter(path, syncTrack, new(config, null));
        writer.Write();
    }
    /// <inheritdoc cref="ReplaceSyncTrack(string, SyncTrack, WritingConfiguration?)"/>
    public static async Task ReplaceSyncTrackAsync(string path, SyncTrack syncTrack, ChartWritingConfiguration? config = default, CancellationToken cancellationToken = default)
    {
        var writer = GetSyncTrackWriter(path, syncTrack, new(config, null));
        await writer.WriteAsync(cancellationToken);
    }
    private static ChartFileWriter GetSyncTrackWriter(string path, SyncTrack syncTrack, ChartWritingSession session) => new(path, null, new SyncTrackSerializer(syncTrack, session));
    #endregion

    /// <summary>
    /// Gets all the combinations of instruments and difficulties.
    /// </summary>
    /// <param name="instruments">Enum containing the instruments</param>
    private static IEnumerable<(Difficulty difficulty, TInstEnum instrument)> GetTrackCombinations<TInstEnum>(IEnumerable<TInstEnum> instruments) => from difficulty in EnumCache<Difficulty>.Values from instrument in instruments select (difficulty, instrument);
}
