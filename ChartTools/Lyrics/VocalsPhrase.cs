using ChartTools.Events;

namespace ChartTools.Lyrics;

public class VocalsPhrase(PhraseMarker marker, IReadOnlyList<VocalsNote> notes) : ILongTrackObject
{
    public PhraseMarker PhraseMarker { get; } = marker;

    public IReadOnlyList<VocalsNote> Notes { get; } = notes;

    public uint Position
    {
        get => PhraseMarker.Position;
        set => PhraseMarker.Position = value;
    }

    public uint Length
    {
        get => PhraseMarker.Length;
        set => PhraseMarker.Length = value;
    }

    public string RawText => BuildText(n =>  n.RawText);
    public string DisplayedText => BuildText(n =>  n.DisplayedText);

    public static IEnumerable<VocalsPhrase> Create(IEnumerable<PhraseMarker> phraseMarkers, IEnumerable<VocalsNote> notes) => throw new NotImplementedException();

    private string BuildText(Func<VocalsNote, string> textSelector) => string.Concat(Notes.Select(n => n.IsWordEnd ? textSelector(n) + ' ' : textSelector(n)));

    public IEnumerable<GlobalEvent> ToGlobalEvents()
    {
        yield return new(Position, EventTypeHelper.Global.PhraseStart);

        foreach (var note in Notes)
            yield return new(note.Position, EventTypeHelper.Global.Lyric, note.RawText);

        uint
            phraseEnd = (PhraseMarker as ILongTrackObject).EndPosition,
            noteEnd = (Notes[^1] as ILongTrackObject).EndPosition;

        if (phraseEnd > noteEnd)
            yield return new(phraseEnd, EventTypeHelper.Global.PhraseEnd);
    }
}

/// <summary>
/// Provides additional methods to <see cref="Phrase"/>
/// </summary>
public static class PhraseExtensions
{
    /// <summary>
    /// Converts a set of <see cref="VocalsPhrase"/> to a set of <see cref="GlobalEvent"/> making up the phrases.
    /// </summary>
    /// <param name="source">Phrases to convert into global events</param>
    /// <returns>Global events making up the phrases</returns>
    public static IEnumerable<GlobalEvent> ToGlobalEvents(this IEnumerable<VocalsPhrase> source) => source.SelectMany(p => p.ToGlobalEvents());
}