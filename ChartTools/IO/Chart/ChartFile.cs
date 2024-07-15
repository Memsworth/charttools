using ChartTools.Events;
using ChartTools.Extensions;
using ChartTools.Extensions.Linq;
using ChartTools.IO.Chart.Configuration;
using ChartTools.IO.Chart.Configuration.Sessions;
using ChartTools.IO.Chart.Parsing;
using ChartTools.IO.Chart.Serializing;
using ChartTools.IO.Components;
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
        var session = new ChartReadingSession(ComponentList.Full(), config, formatting);
        var reader = new ChartFileReader(path, session);

        reader.Read();
        return CreateSongFromReader(reader);
    }

    public static async Task<Song> ReadSongAsync(string path, ChartReadingConfiguration? config = default, FormattingRules? formatting = default, CancellationToken cancellationToken = default)
    {
        var session = new ChartReadingSession(ComponentList.Full(), config, formatting);
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

    public static async Task<Song> ReadComponentsAsync(string path, ComponentList components, ChartReadingConfiguration? config = default, FormattingRules? formatting = default, CancellationToken cancellationToken = default)
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
    public static Instrument? ReadInstrument(string path, InstrumentIdentity instrument, DifficultySet difficulties = DifficultySet.All, ChartReadingConfiguration? config = default, FormattingRules? formatting = default)
    {
        if (instrument == InstrumentIdentity.Drums)
            return ReadDrums(path, difficulties, config, formatting);
        if (Enum.IsDefined((GHLInstrumentIdentity)instrument))
            return ReadInstrument(path, (GHLInstrumentIdentity)instrument, difficulties, config, formatting);
        return Enum.IsDefined((StandardInstrumentIdentity)instrument)
            ? ReadInstrument(path, (StandardInstrumentIdentity)instrument, difficulties, config, formatting)
            : throw new UndefinedEnumException(instrument);
    }

    public static async Task<Instrument?> ReadInstrumentAsync(string path, InstrumentIdentity instrument, DifficultySet difficulties = DifficultySet.All, ChartReadingConfiguration? config = default, FormattingRules? formatting = default, CancellationToken cancellationToken = default)
    {
        if (instrument == InstrumentIdentity.Drums)
            return await ReadDrumsAsync(path, difficulties, config, formatting, cancellationToken);
        if (Enum.IsDefined((GHLInstrumentIdentity)instrument))
            return await ReadInstrumentAsync(path, (GHLInstrumentIdentity)instrument, difficulties, config, formatting, cancellationToken);
        return Enum.IsDefined((StandardInstrumentIdentity)instrument)
            ? await ReadInstrumentAsync(path, (StandardInstrumentIdentity)instrument, difficulties, config, formatting, cancellationToken)
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
    public static Drums? ReadDrums(string path, DifficultySet difficulties = DifficultySet.All, ChartReadingConfiguration ? config = default, FormattingRules? formatting = default)
    {
        var session = new ChartReadingSession(new() { Instruments = new(InstrumentIdentity.Drums, difficulties) }, config, formatting);
        var reader = new ChartFileReader(path, session);

        reader.Read();
        return CreateInstrumentFromReader<Drums, DrumsChord>(reader);
    }

    public static async Task<Drums?> ReadDrumsAsync(string path, DifficultySet difficulties = DifficultySet.All, ChartReadingConfiguration? config = default, FormattingRules? formatting = default, CancellationToken cancellationToken = default)
    {
        var session = new ChartReadingSession(new() { Instruments = new(InstrumentIdentity.Drums) }, config, formatting);
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
    public static GHLInstrument? ReadInstrument(string path, GHLInstrumentIdentity instrument, DifficultySet difficulties = DifficultySet.All, ChartReadingConfiguration? config = default, FormattingRules? formatting = default)
    {
        var session = new ChartReadingSession(new() { Instruments = new(instrument, difficulties) }, config, formatting);
        var reader  = new ChartFileReader(path, session);

        reader.Read();
        return CreateInstrumentFromReader<GHLInstrument, GHLChord>(reader);
    }

    public static async Task<GHLInstrument?> ReadInstrumentAsync(string path, GHLInstrumentIdentity instrument, DifficultySet difficulties = DifficultySet.All, ChartReadingConfiguration? config = default, FormattingRules? formatting = default, CancellationToken cancellationToken = default)
    {
        var session = new ChartReadingSession(new() { Instruments = new(instrument, difficulties) }, config, formatting);
        var reader = new ChartFileReader(path, session);

        await reader.ReadAsync(cancellationToken);
        return CreateInstrumentFromReader<GHLInstrument, GHLChord>(reader);
    }
    #endregion

    #region Standard
    public static StandardInstrument? ReadInstrument(string path, StandardInstrumentIdentity instrument, DifficultySet difficulties = DifficultySet.All, ChartReadingConfiguration? config = default, FormattingRules? formatting = default)
    {
        var session = new ChartReadingSession(new() { Instruments = new(instrument, difficulties) }, config, formatting);
        var reader = new ChartFileReader(path, session);

        reader.Read();
        return CreateInstrumentFromReader<StandardInstrument, StandardChord>(reader);
    }

    public static async Task<StandardInstrument?> ReadInstrumentAsync(string path, StandardInstrumentIdentity instrument, DifficultySet difficulties = DifficultySet.All, ChartReadingConfiguration? config = default, FormattingRules? formatting = default, CancellationToken cancellationToken = default)
    {
        var session = new ChartReadingSession(new() { Instruments = new(instrument, difficulties) }, config, formatting);
        var reader = new ChartFileReader(path, session);

        await reader.ReadAsync(cancellationToken);
        return CreateInstrumentFromReader<StandardInstrument, StandardChord>(reader);
    }
    #endregion
    #endregion

    #region Tracks
    [Obsolete($"Use {nameof(ReadInstrument)} with a {nameof(DifficultySet)}.")]
    public static Track? ReadTrack(string path, InstrumentIdentity instrument, Difficulty difficulty, ChartReadingConfiguration? config = default, FormattingRules? formatting = default)
        => ReadInstrument(path, instrument, difficulty.ToSet(), config, formatting)?.GetTrack(difficulty);

    [Obsolete($"Use {nameof(ReadInstrumentAsync)} with a {nameof(DifficultySet)}.")]
    public static async Task<Track?> ReadTrackAsync(string path, InstrumentIdentity instrument, Difficulty difficulty, ChartReadingConfiguration? config = default, FormattingRules? formatting = default, CancellationToken cancellationToken = default)
        => (await ReadInstrumentAsync(path, instrument, difficulty.ToSet(), config, formatting, cancellationToken))?.GetTrack(difficulty);

    #region Drums
    [Obsolete($"Use {nameof(ReadDrums)} with a {nameof(DifficultySet)}.")]
    public static Track<DrumsChord> ReadDrumsTrack(string path, Difficulty difficulty, ChartReadingConfiguration? config = default, FormattingRules? formatting = default)
        => ReadDrums(path, difficulty.ToSet(), config, formatting)?.GetTrack(difficulty) ?? new();

    [Obsolete($"Use {nameof(ReadDrumsAsync)} with a {nameof(DifficultySet)}.")]
    public static async Task<Track<DrumsChord>> ReadDrumsTrackAsync(string path, Difficulty difficulty, ChartReadingConfiguration? config = default, FormattingRules? formatting = default, CancellationToken cancellationToken = default)
        => (await ReadDrumsAsync(path, difficulty.ToSet(), config, formatting, cancellationToken))?.GetTrack(difficulty) ?? new();
    #endregion

    #region GHL
    [Obsolete($"Use {nameof(ReadInstrument)} with a {nameof(DifficultySet)}.")]
    public static Track<GHLChord>? ReadTrack(string path, GHLInstrumentIdentity instrument, Difficulty difficulty, ChartReadingConfiguration? config = default, FormattingRules? formatting = default)
        => ReadInstrument(path, instrument, difficulty.ToSet(), config, formatting)?.GetTrack(difficulty);

    [Obsolete($"Use {nameof(ReadInstrumentAsync)} with a {nameof(DifficultySet)}.")]
    public static async Task<Track<GHLChord>?> ReadTrackAsync(string path, GHLInstrumentIdentity instrument, Difficulty difficulty, ChartReadingConfiguration? config, FormattingRules? formatting = default, CancellationToken cancellationToken = default)
        => (await ReadInstrumentAsync(path, instrument, difficulty.ToSet(), config, formatting, cancellationToken))?.GetTrack(difficulty);
    #endregion

    #region Standard
    [Obsolete($"Use {nameof(ReadInstrument)} with a {nameof(DifficultySet)}.")]
    public static Track<StandardChord>? ReadTrack(string path, StandardInstrumentIdentity instrument, Difficulty difficulty, ChartReadingConfiguration? config = default, FormattingRules? formatting = default)
        => ReadInstrument(path, instrument, difficulty.ToSet(), config, formatting)?.GetTrack(difficulty);

    [Obsolete($"Use {nameof(ReadInstrumentAsync)} with a {nameof(DifficultySet)}.")]
    public static async Task<Track<StandardChord>?> ReadTrackAsync(string path, StandardInstrumentIdentity instrument, Difficulty difficulty, ChartReadingConfiguration? config = default, FormattingRules? formatting = default, CancellationToken cancellationToken = default)
         => (await ReadInstrumentAsync(path, instrument, difficulty.ToSet(), config, formatting, cancellationToken))?.GetTrack(difficulty);
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
    private static void FillInstrumentWriterData(Instrument? inst, InstrumentIdentity identity, DifficultySet tracks, ChartWritingSession session,
        ICollection<Serializer<string>> serializers, ICollection<string> removedHeaders)
    {
        // Only act on tracks specified in the component list
        foreach (var diff in EnumCache<Difficulty>.Values.Where(d => tracks.HasFlag(d.ToSet())))
        {
            var track = inst?.GetTrack(diff);

            if (track?.IsEmpty is not null or false)
                serializers.Add(new TrackSerializer(track, session));
            else // No track data for the instrument and difficulty
                removedHeaders.Add(ChartFormatting.Header(identity, diff));
        }
    }

    private static ChartFileWriter GetSongWriter(string path, Song song, ComponentList components, ChartWritingSession session)
    {
        var removedHeaders = new List<string>();
        var serializers = new List<Serializer<string>>();

        if (components.Metadata)
            serializers.Add(new MetadataSerializer(song.Metadata));

        if (components.SyncTrack)
        {
            if (!song.SyncTrack.IsEmpty)
                serializers.Add(new SyncTrackSerializer(song.SyncTrack, session));
            else
                removedHeaders.Add(ChartFormatting.SyncTrackHeader);
        }

        if (components.GlobalEvents)
        {
            if (song.GlobalEvents.Count > 0)
                serializers.Add(new GlobalEventSerializer(song.GlobalEvents, session));
            else
                removedHeaders.Add(ChartFormatting.GlobalEventHeader);
        }

        foreach (var identity in EnumCache<InstrumentIdentity>.Values)
            FillInstrumentWriterData(song.Instruments.Get(identity), identity, components.Instruments.Map(identity), session, serializers, removedHeaders);

        if (song.UnknownChartSections is not null)
            serializers.AddRange(song.UnknownChartSections.Select(s => new UnknownSectionSerializer(s.Header, s, session)));

        return new(path, removedHeaders, [.. serializers]);
    }

    /// <summary>
    /// Writes a song to a chart file.
    /// </summary>
    /// <param name="path">Path of the file to write</param>
    /// <param name="song">Song to write</param>
    public static void WriteSong(string path, Song song, ChartWritingConfiguration? config = default)
    {
        var writer = GetSongWriter(path, song, ComponentList.Full(), new(config, song.Metadata.Formatting));
        writer.Write();
    }

    public static async Task WriteSongAsync(string path, Song song, ChartWritingConfiguration? config = default, CancellationToken cancellationToken = default)
    {
        var writer = GetSongWriter(path, song, ComponentList.Full(), new(config, song.Metadata.Formatting));
        await writer.WriteAsync(cancellationToken);
    }

    public static void ReplaceComponents(string path, Song song, ComponentList components, ChartWritingConfiguration? config = default)
    {
        var writer = GetSongWriter(path, song, components, new(config, song.Metadata.Formatting));
        writer.Write();
    }

    public static async Task ReplaceComponentsAsync(string path, Song song, ComponentList components, ChartWritingConfiguration? config = default, CancellationToken cancellationToken = default)
    {
        var writer = GetSongWriter(path, song, components, new(config, song.Metadata.Formatting));
        await writer.WriteAsync(cancellationToken);
    }

    private static ChartFileWriter GetInstrumentWriter(string path, Instrument? instrument, InstrumentIdentity identity, ChartWritingSession session)
    {
        var removedHeaders = new List<string>();
        var serializers = new List<Serializer<string>>();

        FillInstrumentWriterData(instrument, identity, DifficultySet.All, session, serializers, removedHeaders);

        return new(path, removedHeaders, [.. serializers]);
    }

    /// <summary>
    /// Replaces an instrument in a file.
    /// </summary>
    /// <param name="path">Path of the file to write</param>
    public static void ReplaceInstrument(string path, Instrument instrument, ChartWritingConfiguration? config = default, FormattingRules? formatting = default)
    {
        var writer = GetInstrumentWriter(path, instrument, instrument.InstrumentIdentity, new(config, formatting));
        writer.Write();
    }

    public static async Task ReplaceInstrumentAsync(string path, Instrument instrument, ChartWritingConfiguration? config = default, FormattingRules? formatting = default, CancellationToken cancellationToken = default)
    {
        var writer = GetInstrumentWriter(path, instrument, instrument.InstrumentIdentity, new(config, formatting));
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

    public static void ReplaceTrack(string path, Track track, ChartWritingConfiguration? config = default, FormattingRules? formatting = default)
    {
        var writer = GetTrackWriter(path, track, new(config, formatting));
        writer.Write();
    }

    public static async Task ReplaceTrackAsync(string path, Track track, ChartWritingConfiguration? config = default, FormattingRules? formatting = default, CancellationToken cancellationToken = default)
    {
        var writer = GetTrackWriter(path, track, new(config, formatting));
        await writer.WriteAsync(cancellationToken);
    }

    private static ChartFileWriter GetMetadataWriter(string path, Metadata metadata) => new(path, null, new MetadataSerializer(metadata));

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

    private static ChartFileWriter GetGlobalEventWriter(string path, IEnumerable<GlobalEvent> events, ChartWritingSession session) => new(path, null, new GlobalEventSerializer(events, session));

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

    private static ChartFileWriter GetSyncTrackWriter(string path, SyncTrack syncTrack, ChartWritingSession session) => new(path, null, new SyncTrackSerializer(syncTrack, session));

    /// <summary>
    /// Replaces the sync track in a file.
    /// </summary>
    /// <param name="path">Path of the file to write</param>
    /// <param name="syncTrack">Sync track to write</param>
    /// <param name="config"><inheritdoc cref="ReadingConfiguration" path="/summary"/></param>
    public static void ReplaceSyncTrack(string path, SyncTrack syncTrack, ChartWritingConfiguration? config = default)
    {
        var writer = GetSyncTrackWriter(path, syncTrack, new( config, null));
        writer.Write();
    }

    public static async Task ReplaceSyncTrackAsync(string path, SyncTrack syncTrack, ChartWritingConfiguration? config = default, CancellationToken cancellationToken = default)
    {
        var writer = GetSyncTrackWriter(path, syncTrack, new(config, null));
        await writer.WriteAsync(cancellationToken);
    }
    #endregion

    /// <summary>
    /// Gets all the combinations of instruments and difficulties.
    /// </summary>
    /// <param name="instruments">Enum containing the instruments</param>
    private static IEnumerable<(Difficulty difficulty, TInstEnum instrument)> GetTrackCombinations<TInstEnum>(IEnumerable<TInstEnum> instruments) => from difficulty in EnumCache<Difficulty>.Values from instrument in instruments select (difficulty, instrument);
}
