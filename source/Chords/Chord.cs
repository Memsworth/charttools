﻿using ChartTools.IO.Chart;

using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace ChartTools
{
    /// <summary>
    /// Base class for chords
    /// </summary>
    public abstract class Chord : TrackObject, IEnumerable<Note>
    {
        /// <summary>
        /// Notes in the <see cref="Chord{TNote}"/>
        /// </summary>
        public abstract IEnumerable<Note> Notes { get; }

        /// <param name="position">Position of the Chord on the Track</param>
        protected Chord(uint position) : base(position) { }

        /// <summary>
        /// Gets the data to write in a chart file.
        /// </summary>
        /// <returns>Enumerable of strings containing the data to add to each line</returns>
        internal abstract IEnumerable<string> GetChartNoteData();
        internal abstract IEnumerable<string> GetChartModifierData(Chord? previous, ChartParser.WritingSession session);
        internal IEnumerable<string> GetChartData(Chord? previous, ChartParser.WritingSession session) => GetChartNoteData().Concat(GetChartModifierData(previous, session));
        internal abstract bool ChartSupportedMoridier { get; }

        public IEnumerator<Note> GetEnumerator() => Notes.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}