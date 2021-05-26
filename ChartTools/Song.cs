﻿using ChartTools.IO;
using ChartTools.IO.Chart;
using ChartTools.IO.Ini;
using ChartTools.IO.MIDI;
using ChartTools.Lyrics;

using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace ChartTools
{
    /// <summary>
    /// Song playable in Clone Hero
    /// </summary>
    public class Song
    {
        #region Properties
        /// <summary>
        /// Set of information about the song not unrelated to instruments, syncing or events
        /// </summary>
        public Metadata Metadata { get; set; } = new Metadata();
        /// <inheritdoc cref="ChartTools.SyncTrack"/>
        public SyncTrack SyncTrack { get; set; } = new SyncTrack();
        /// <summary>
        /// List of events common to all instruments
        /// </summary>
        public List<GlobalEvent> GlobalEvents { get; set; } = new List<GlobalEvent>();

        /// <summary>
        /// Set of drums tracks
        /// </summary>
        public Instrument<DrumsChord> Drums { get; set; }
        /// <summary>
        /// Set of Guitar Hero Live guitar tracks
        /// </summary>
        public Instrument<GHLChord> GHLGuitar { get; set; }
        /// <summary>
        /// Set of Guitar Hero Live bass tracks
        /// </summary>
        public Instrument<GHLChord> GHLBass { get; set; }
        /// <summary>
        /// Set of lead guitar tracks
        /// </summary>
        public Instrument<StandardChord> LeadGuitar { get; set; }
        /// <summary>
        /// Set of rythym guitar tracks
        /// </summary>
        public Instrument<StandardChord> RhythmGuitar { get; set; }
        /// <summary>
        /// Set of coop guitar tracks
        /// </summary>
        public Instrument<StandardChord> CoopGuitar { get; set; }
        /// <summary>
        /// Set of bass tracks
        /// </summary>
        public Instrument<StandardChord> Bass { get; set; }
        /// <summary>
        /// Set of keyboard tracks
        /// </summary>
        public Instrument<StandardChord> Keys { get; set; }
        #endregion

        /// <summary>
        /// Gets property value for an <see cref="Instrument"/> from a <see cref="Instruments"/> <see langword="enum"/> value.
        /// </summary>
        /// <returns>Instance of <see cref="Instrument"/> from the <see cref="Song"/></returns>
        /// <param name="instrument">Instrument to get</param>
        public Instrument GetInstrument(Instruments instrument) => instrument switch
        {
            Instruments.Drums => Drums,
            Instruments.GHLGuitar => GHLGuitar,
            Instruments.GHLBass => GHLBass,
            Instruments.LeadGuitar => LeadGuitar,
            Instruments.RhythmGuitar => RhythmGuitar,
            Instruments.CoopGuitar => CoopGuitar,
            Instruments.Bass => Bass,
            Instruments.Keys => Keys,
            _ => throw new Exception("Instrument does not exist.")
        };
        /// <summary>
        /// Gets property value for an <see cref="Instrument{TChord}"/> from a <see cref="GHLInstrument"/> <see langword="enum"/> value.
        /// </summary>
        /// /// <param name="instrument">Instrument to get</param>
        /// <returns>Instance of <see cref="Instrument{TChord}"/> where TChord is <see cref="GHLChord"/> from the <see cref="Song"/>.</returns>
        public Instrument<GHLChord> GetInstrument(GHLInstrument instrument) => GetInstrument((Instruments)instrument) as Instrument<GHLChord>;
        /// <summary>
        /// Gets property value for an <see cref="Instrument{TChord}"/> from a <see cref="StandardInstrument"/> <see langword="enum"/> value.
        /// </summary>
        /// <param name="instrument">Instrument to get</param>
        /// <returns>Instance of <see cref="Instrument{TChord}"/> where TChord is <see cref="StandardChord"/> from the <see cref="Song"/>.</returns>
        public Instrument<StandardChord> GetInstrument(StandardInstrument instrument) => GetInstrument((Instruments)instrument) as Instrument<StandardChord>;

        /// <summary>
        /// Reads a <see cref="Song"/> from a file.
        /// </summary>
        /// <remarks>Supported extentions: chart, mid, ini</remarks>
        /// <exception cref="ArgumentException"/>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="FormatException"/>
        /// <exception cref="IOException"/>
        /// <exception cref="OutOfMemoryException"/>
        /// <exception cref="CommonExceptions.ParameterNullException"/>
        public static Song FromFile(string path)
        {
            try { return FromFile(path, new()); }
            catch { throw; }
        }
        /// <inheritdoc cref="FromFile(string)"/>
        public static Song FromFile(string path, ReadingConfiguration config)
        {
            try { return ExtensionHandler.Read(path, config, (".chart", ChartParser.ReadSong), (".mid", MIDIParser.ReadSong), (".ini", (p, config) => new Song { Metadata = IniParser.ReadMetadata(p) })); }
            catch { throw; }
        }
        /// <summary>
        /// Writes the <see cref="Song"/> to a file.
        /// </summary>
        /// <exception cref="ArgumentException"/>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="PathTooLongException"/>
        /// <exception cref="DirectoryNotFoundException"/>
        /// <exception cref="IOException"/>
        /// <exception cref="UnauthorizedAccessException"/>
        /// <exception cref="NotSupportedException"/>
        /// <exception cref="System.Security.SecurityException"/>
        public void ToFile(string path)
        {
            try { ToFile(path, new()); }
            catch { throw; }
        }
        /// <inheritdoc cref="ToFile(string)"/>
        public void ToFile(string path, WritingConfiguration config)
        {
            try { ExtensionHandler.Write(path, this, config, (".chart", ChartParser.WriteSong)); }
            catch { throw; }
        }

        /// <summary>
        /// Reads the estimated instrument difficulties from a ini file.
        /// </summary>
        /// <param name="path">Path of the file to read</param>
        /// <exception cref="ArgumentException"/>
        /// <exception cref="FormatException"/>
        /// <exception cref="IOException"/>
        public void ReadDifficulties(string path)
        {
            try { ExtensionHandler.Read(path, (".ini", p => IniParser.ReadDifficulties(p, this))); }
            catch { throw; }

        }
        /// <summary>
        /// Writes the estimated instrument difficulties to a ini file.
        /// </summary>
        /// <param name="path">Path of the file to write</param>
        /// <exception cref="ArgumentException"/>
        /// <exception cref="IOException"/>
        public void WriteDifficulties(string path)
        {
            try { ExtensionHandler.Write(path, this, (".ini", IniParser.WriteDifficulties)); }
            catch { throw; }
        }

        /// <summary>
        /// Retrieves the lyrics from the global events.
        /// </summary>
        public IEnumerable<Phrase> GetLyrics() => GlobalEvents is null ? Enumerable.Empty<Phrase>() : GlobalEvents.GetLyrics();
        /// <summary>
        /// Replaces phrase and lyric events from <see cref="GlobalEvents"/> with the ones making up a set of <see cref="Phrase"/>.
        /// </summary>
        /// <param name="phrases">Phrases to use as a replacement</param>
        public void SetLyrics(IEnumerable<Phrase> phrases) => GlobalEvents = GlobalEvents.SetLyrics(phrases).ToList();
    }
}