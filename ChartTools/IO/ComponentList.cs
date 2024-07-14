namespace ChartTools.IO;

[Flags]
public enum DifficultySet : byte
{
    None = 0,
    Easy = 1 << 0,
    Medium = 1 << 1,
    Hard = 1 << 2,
    Expert = 1 << 3,
    All = Easy | Medium | Hard | Expert
};

public record ComponentList
{
    public bool Metadata { get; set; }
    public bool SyncTrack { get; set; }
    public bool GlobalEvents { get; set; }

    public DifficultySet Drums { get; set; }
    public DifficultySet LeadGuitar { get; set; }
    public DifficultySet CoopGuitar { get; set; }
    public DifficultySet RythmGuitar { get; set; }
    public DifficultySet Bass { get; set; }
    public DifficultySet GHLGuitar { get; set; }
    public DifficultySet GHLBass { get; set; }
    public DifficultySet Keys { get; set; }
    public DifficultySet Vocals { get; set; }
}
