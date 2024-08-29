namespace ChartTools.Lyrics;

public class PhraseMarker(uint position) : TrackObjectBase(position), ILongTrackObject
{
    public uint Length { get; set; }
}
