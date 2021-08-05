﻿using ChartTools.IO.Chart;
using System;
using System.Linq;

namespace ChartTools
{
    /// <summary>
    /// Set of notes played simultaneously by drums
    /// </summary>
    public class DrumsChord : Chord<DrumsNote, DrumsNotes>
    {
        /// <inheritdoc cref="DrumsChordModifier"/>
        public DrumsChordModifier Modifier { get; set; } = DrumsChordModifier.None;
        protected override bool openExclusivity => false;

        /// <inheritdoc cref="Chord(uint)"/>
        /// <param name="notes">Notes to add</param>
        public DrumsChord(uint position, params DrumsNote[] notes) : base(position)
        {
            if (notes is null)
                throw new ArgumentNullException(nameof(notes));

            foreach (DrumsNote note in notes)
                Notes.Add(note);
        }
        /// <inheritdoc cref="DrumsChord(uint, DrumsNote[])"/>
        public DrumsChord(uint position, params DrumsNotes[] notes) : base(position)
        {
            if (notes is null)
                throw new ArgumentNullException(nameof(notes));

            foreach (DrumsNotes note in notes)
                Notes.Add(new DrumsNote(note));
        }

        /// <inheritdoc/>
        internal override System.Collections.Generic.IEnumerable<string> GetChartData()
        {
            foreach (DrumsNote note in Notes)
            {
                yield return ChartParser.GetNoteData(note.Note == DrumsNotes.DoubleKick ? (byte)32 : (byte)note.Note, note.SustainLength);

                if (note.IsCymbal)
                    yield return ChartParser.GetNoteData((byte)(note.Note + 64), 0);
            }

            if (Modifier != DrumsChordModifier.None)
            {
                // Add once accent and ghost are added to Clone Hero
            }
        }
    }
}
