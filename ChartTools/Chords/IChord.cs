﻿namespace ChartTools;

/// <summary>
/// Set of notes tied together.
/// </summary>
/// <remarks>Depending on the chord type, notes will be on the same position.</remarks>
public interface IChord : ITrackObject
{
    /// <summary>
    /// Read-only set of the notes in the chord.
    /// </summary>
    public IReadOnlyCollection<INote> Notes { get; }

    public INote CreateNote(byte index, uint length = 0);
}
