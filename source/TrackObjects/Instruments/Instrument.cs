﻿using ChartTools.IO;
using ChartTools.IO.Chart;
using ChartTools.IO.Ini;
using System;
using DiffEnum = ChartTools.Difficulty;

namespace ChartTools
{
    /// <summary>
    /// Base class for instruments
    /// </summary>
    public abstract class Instrument
    {
        /// <summary>
        /// Estimated difficulty
        /// </summary>
        public sbyte? Difficulty { get; set; }

        private const string undefinedInstrumentMessage = "Instrument is not defined.";

        protected abstract Track? GetEasy();
        protected abstract Track? GetMedium();
        protected abstract Track? GetHard();
        protected abstract Track? GetExpert();
        public Track? GetTrack(DiffEnum difficulty) => difficulty switch
        {
            DiffEnum.Easy => GetEasy(),
            DiffEnum.Medium => GetMedium(),
            DiffEnum.Hard => GetHard(),
            DiffEnum.Expert => GetExpert(),
            _ => throw CommonExceptions.GetUndefinedException(difficulty)
        };

        /// <inheritdoc cref="ChartParser.ReadInstrument(string, Instruments)"/>
        public static Instrument FromFile(string path, Instruments instrument, ReadingConfiguration config) => Enum.IsDefined(instrument)
            ? ExtensionHandler.Read(path, config, (".chart", (p, config) => ChartParser.ReadInstrument(p, instrument, config)))
            : throw new ArgumentException(undefinedInstrumentMessage);
        /// <inheritdoc cref="ChartParser.ReadDrums(string)"/>
        public static Instrument<DrumsChord> DrumsFromFile(string path, ReadingConfiguration config) => ExtensionHandler.Read(path, config, (".chart", ChartParser.ReadDrums));
        /// <inheritdoc cref="ChartParser.ReadInstrument(string, GHLInstrument)"/>
        public static Instrument<GHLChord> FromFile(string path, GHLInstrument instrument, ReadingConfiguration config) => Enum.IsDefined(instrument)
            ? ExtensionHandler.Read(path, config, (".chart", (p, config) => ChartParser.ReadInstrument(p, instrument, config)))
            : throw new ArgumentException(undefinedInstrumentMessage);

        /// <inheritdoc cref="ChartParser.ReadInstrument(string, StandardInstrument)"/>
        public static Instrument<StandardChord> FromFile(string path, StandardInstrument instrument, ReadingConfiguration config) => Enum.IsDefined(instrument)
            ? ExtensionHandler.Read(path, config, (".chart", (p, config) => ChartParser.ReadInstrument(p, instrument, config)))
            : throw new ArgumentException(undefinedInstrumentMessage);

        /// <inheritdoc cref="IniParser.ReadDifficulty(string, Instruments)"/>
        public static sbyte? ReadDifficulty(string path, Instruments instrument) => ExtensionHandler.Read(path, (".ini", p => IniParser.ReadDifficulty(p, instrument)));
        /// <inheritdoc cref="IniParser.WriteDifficulty(string, Instruments, sbyte)"/>
        public static void WriteDifficulty(string path, Instruments instrument, sbyte value) => ExtensionHandler.Write(path, value, (".ini", (p, v) => IniParser.WriteDifficulty(p, instrument, v)));
    }
}
