﻿namespace ChartTools
{
    /// <summary>
    /// Base class for notes
    /// </summary>
    public abstract class Note : ILongObject
    {
        /// <summary>
        /// Numerical value of the note as written in a chart file
        /// </summary>
        internal abstract byte NoteIndex { get; }
        /// <summary>
        /// Maximum length the note can be held for extra points
        /// </summary>
        public uint Length { get; set; } = 0;
    }
}