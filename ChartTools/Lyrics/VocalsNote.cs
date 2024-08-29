namespace ChartTools.Lyrics;

public class VocalsNote(VocalsPitch pitch) : INote
{
    public VocalsPitch Pitch { get; set; } = pitch;

    byte INote.Index => (byte)Pitch.Value;

    public uint Length { get; set; }

    public string RawText { get; set; }

    public string DisplayedText => RawText;

    public VocalsNote() : this(VocalsPitchValue.None) { }
}
