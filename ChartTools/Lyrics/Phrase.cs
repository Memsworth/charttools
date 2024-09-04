﻿using ChartTools.Events;
using ChartTools.Extensions.Linq;

namespace ChartTools.Lyrics;

/// <summary>
/// Grouping of a <see cref="Lyrics.PhraseMarker"/> and set of <see cref="VocalsNote"/> for assembling lyric text.
/// </summary>
/// <param name="marker"></param>
/// <param name="notes"></param>
public class Phrase(PhraseMarker marker, IReadOnlyList<VocalsNote>? notes = null) : ILongTrackObject
{
    public PhraseMarker PhraseMarker { get; } = marker;

    public IReadOnlyList<VocalsNote> Notes { get; } = notes ?? [];

    public uint Position
    {
        get => PhraseMarker.Position;
        set => PhraseMarker.Position = value;
    }

    /// <inheritdoc cref="PhraseMarker.Length"/>
    public uint Length
    {
        get => PhraseMarker.Length;
        set => PhraseMarker.Length = value;
    }

    public string RawText => BuildText(n => n.RawText);
    public string DisplayedText => BuildText(n => n.DisplayedText);

    public static IEnumerable<Phrase> Create(IEnumerable<PhraseMarker> phraseMarkers, IEnumerable<VocalsNote> notes) => throw new NotImplementedException();

    private string BuildText(Func<VocalsNote, string> textSelector) => string.Concat(Notes.Select(n => n.IsWordEnd ? textSelector(n) + ' ' : textSelector(n)));

    public IEnumerable<GlobalEvent> ToGlobalEvents()
    {
        yield return new(Position, EventTypeHelper.Global.PhraseStart);

        foreach (var note in Notes)
            yield return new(note.Position, EventTypeHelper.Global.Lyric, note.RawText);

        if (PhraseMarker.Length > 0)
            yield return new((PhraseMarker as ILongTrackObject).EndPosition, EventTypeHelper.Global.PhraseEnd);
    }
}

/// <summary>
/// Provides additional methods to <see cref="Phrase"/>.
/// </summary>
public static class PhraseExtensions
{
    public static void GetLyrics(this IEnumerable<GlobalEvent> events, out IList<PhraseMarker> phrases, out IList<VocalsNote> notes)
    {
        PhraseMarker? phrase = null;

        phrases = [];
        notes = [];

        foreach (var e in events.OrderBy(e => e.Position))
        {
            switch (e.EventType)
            {
                case EventTypeHelper.Global.PhraseStart:
                    phrase = new(e.Position);
                    phrases.Add(phrase);
                    break;
                case EventTypeHelper.Global.Lyric:
                    notes.Add(new(e.Argument));
                    break;
                case EventTypeHelper.Global.PhraseEnd:
                    if (phrase is not null)
                        phrase.Length = e.Position - phrase.Position;
                    break;
            }
        }
    }

    public static IEnumerable<Phrase> GetLyrics(IEnumerable<PhraseMarker> phrases, IEnumerable<VocalsNote> notes)
    {
        using var phraseEnumerator = phrases.OrderBy(p => p.Position).GetEnumerator();

        if (!phraseEnumerator.MoveNext())
            yield break;

        using var notesEnumerator = notes.OrderBy(n => n.Position).GetEnumerator();
        notesEnumerator.MoveNext(); // Initialize prematurely to simplify the loop flow

        PhraseMarker lastMarker = phraseEnumerator.Current;
        List<VocalsNote> lastPhraseNotes = [];

        // Keeps track on if the notes enumerator reached the end as IEnumerator provides no safe way of checking without mutating
        bool notesRemaining = true;

        while (phraseEnumerator.MoveNext()) // Peek forward and add prior notes to the last phrase
        {
            while (notesRemaining && notesEnumerator.Current.Position < phraseEnumerator.Current.Position)
            {
                lastPhraseNotes.Add(notesEnumerator.Current);
                notesRemaining = notesEnumerator.MoveNext();
            }

            yield return new(lastMarker, lastPhraseNotes);
            lastMarker = phraseEnumerator.Current;
        }

        // Add remaining notes to the last phrase
        while (notesRemaining)
        {
            lastPhraseNotes.Add(notesEnumerator.Current);
            notesRemaining = notesEnumerator.MoveNext();
        }

        yield return new(lastMarker, lastPhraseNotes);
    }

    public static IEnumerable<Phrase> GetLyrics(this IEnumerable<GlobalEvent> events)
    {
        GetLyrics(events, out var phrases, out var notes);
        return GetLyrics(phrases, notes);
    }

    public static IEnumerable<GlobalEvent> ToGlobalEvents(IEnumerable<PhraseMarker> markers, IEnumerable<VocalsNote> notes)
    {
        foreach (var marker in markers)
        {
            yield return new(marker.Position, EventTypeHelper.Global.PhraseStart);

            if (marker.Length > 0)
                yield return new((marker as ILongTrackObject).EndPosition, EventTypeHelper.Global.PhraseEnd);
        }

        foreach (var note in notes)
            yield return new(note.Position, EventTypeHelper.Global.Lyric, note.RawText);
    }

    /// <summary>
    /// Converts a set of <see cref="Phrase"/> to a set of <see cref="GlobalEvent"/> making up the phrases.
    /// </summary>
    /// <param name="source">Phrases to convert into global events</param>
    /// <returns>Global events making up the phrases</returns>
    public static IEnumerable<GlobalEvent> ToGlobalEvents(this IEnumerable<Phrase> source) => source.SelectMany(p => p.ToGlobalEvents());

    public static IEnumerable<GlobalEvent> SetLyrics(this IEnumerable<GlobalEvent> events, IEnumerable<PhraseMarker> markers, IEnumerable<VocalsNote> notes)
    {
        IEnumerable<GlobalEvent>[] collections =
        [
           events.Where(e => !e.IsLyricEvent),
           ToGlobalEvents(markers, notes)
        ];

        return collections.AlternateBy(e => e.Position);
    }

    public static IEnumerable<GlobalEvent> SetLyrics(this IEnumerable<GlobalEvent> events, IEnumerable<Phrase> phrases)
    {
        // TODO Add overload with IList or similar type that can be modified directly by adding and removing events

        IEnumerable<GlobalEvent>[] collections =
        [
            events.Where(e => !e.IsLyricEvent),
            phrases.ToGlobalEvents()
        ];

        return collections.AlternateBy(e => e.Position);
    }
}
