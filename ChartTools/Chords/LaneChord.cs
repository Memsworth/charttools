﻿using ChartTools.IO.Chart.Configuration.Sessions;
using ChartTools.IO.Chart.Entries;

namespace ChartTools;

public abstract class LaneChord(uint position) : IChord
{
    public LaneChord() : this(0) { }

    public uint Position { get; set; } = position;

    public IReadOnlyCollection<LaneNote> Notes => GetNotes();
    IReadOnlyCollection<INote> IChord.Notes => GetNotes();

    /// <summary>
    /// Defines if open notes can be mixed with other notes for this chord type. <see langword="true"/> indicated open notes cannot be mixed.
    /// </summary>
    public abstract bool OpenExclusivity { get; }

    internal abstract bool ChartSupportedModifiers { get; }

    public abstract LaneNote CreateNote(byte index, uint sustain = 0);
    INote IChord.CreateNote(byte index, uint sustain) => CreateNote(index, sustain);

    protected abstract IReadOnlyCollection<LaneNote> GetNotes();

    internal abstract IEnumerable<TrackObjectEntry> GetChartNoteData();

    internal abstract IEnumerable<TrackObjectEntry> GetChartModifierData(LaneChord? previous, ChartWritingSession session);
}
