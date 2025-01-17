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
}
