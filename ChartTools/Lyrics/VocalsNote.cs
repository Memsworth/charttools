namespace ChartTools.Lyrics;

public class VocalsNote(VocalsPitch pitch) : INote, ILongTrackObject
{
    public uint Position { get; set; }

    public uint Length { get; set; }

    public VocalsPitch Pitch { get; set; } = pitch;

    byte INote.Index => (byte)Pitch.Value;

    public string RawText { get; set; }

    /// <summary>
    /// Text formatted to its in-game appearance
    /// </summary>
    public string DisplayedText => RawText.Replace("-", "").Replace('=', '-').Trim('+', '#', '^', '*');

    /// <summary>
    /// <see langword="true"/> if is the last syllable or the only syllable of its word
    /// </summary>
    public bool IsWordEnd
    {
        get => RawText.Length == 0 || RawText[^1] is '§' or '_' or not '-' and not '=';
        set
        {
            if (value)
            {
                if (!IsWordEnd)
                    RawText = RawText[..^1];
            }
            else if (IsWordEnd)
                RawText += '-';
        }
    }

    public VocalsNote() : this(VocalsPitchValue.None) { }
}
