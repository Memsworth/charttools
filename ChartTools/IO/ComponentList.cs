namespace ChartTools.IO;

[Flags]
public enum DifficultySet : byte
{
    None   = 0,
    Easy   = 1 << 0,
    Medium = 1 << 1,
    Hard   = 1 << 2,
    Expert = 1 << 3,
    All = Easy | Medium | Hard | Expert
};

public static class DifficultyExtensions
{
    public static DifficultySet ToSet(this Difficulty difficulty) => (DifficultySet)(1 << (int)difficulty);
}

public record ComponentList()
{
    public static ComponentList Full() => new()
    {
        Metadata     = true,
        SyncTrack    = true,
        GlobalEvents = true,
        Drums        = DifficultySet.All,
        LeadGuitar   = DifficultySet.All,
        CoopGuitar   = DifficultySet.All,
        RythmGuitar  = DifficultySet.All,
        Bass         = DifficultySet.All,
        GHLGuitar    = DifficultySet.All,
        GHLBass      = DifficultySet.All,
        Keys         = DifficultySet.All,
        Vocals       = DifficultySet.All
    };

    public bool Metadata { get; set; }
    public bool SyncTrack { get; set; }
    public bool GlobalEvents { get; set; }

    public DifficultySet Drums
    {
        get => _drums;
        set => _drums = value;
    }
    private DifficultySet _drums;

    public DifficultySet LeadGuitar
    {
        get => _leadGuitar;
        set => _leadGuitar = value;
    }
    private DifficultySet _leadGuitar;

    public DifficultySet CoopGuitar
    {
        get => _coopGuitar;
        set => _coopGuitar = value;
    }
    private DifficultySet _coopGuitar;

    public DifficultySet RythmGuitar
    {
        get => _rythmGuitar;
        set => _rythmGuitar = value;
    }
    private DifficultySet _rythmGuitar;

    public DifficultySet Bass
    {
        get => _bass;
        set => _bass = value;
    }
    private DifficultySet _bass;

    public DifficultySet GHLGuitar
    {
        get => _ghlGuitar;
        set => _ghlGuitar = value;
    }
    private DifficultySet _ghlGuitar;

    public DifficultySet GHLBass
    {
        get => _ghlBass;
        set => _ghlBass = value;
    }
    private DifficultySet _ghlBass;

    public DifficultySet Keys
    {
        get => _keys;
        set => _keys = value;
    }
    private DifficultySet _keys;

    public DifficultySet Vocals
    {
        get => _vocals;
        set => _vocals = value;
    }
    private DifficultySet _vocals;

    public ComponentList(InstrumentIdentity instrument, DifficultySet difficulties = DifficultySet.All) : this()
    {
        Validator.ValidateEnum(instrument);
        Validator.ValidateEnum(difficulties);

        MapInstrument(instrument) = difficulties;
    }

    public ComponentList(StandardInstrumentIdentity instrument, DifficultySet difficulties = DifficultySet.All) : this()
    {
        Validator.ValidateEnum(instrument);
        Validator.ValidateEnum(difficulties);

        MapInstrument((InstrumentIdentity)instrument) = difficulties;
    }

    public ComponentList(GHLInstrumentIdentity instrument, DifficultySet difficulties = DifficultySet.All) : this()
    {
        Validator.ValidateEnum(instrument);
        Validator.ValidateEnum(difficulties);

        MapInstrument((InstrumentIdentity)instrument) = difficulties;
    }

    public ref DifficultySet MapInstrument(InstrumentIdentity instrument)
    {
        switch (instrument)
        {
            case InstrumentIdentity.Drums:
                return ref _drums;
            case InstrumentIdentity.LeadGuitar:
                return ref _leadGuitar;2
            case InstrumentIdentity.CoopGuitar:
                return ref _coopGuitar;
            case InstrumentIdentity.RhythmGuitar:
                return ref _rythmGuitar;
            case InstrumentIdentity.Bass:
                return ref _bass;
            case InstrumentIdentity.GHLGuitar:
                return ref _ghlGuitar;
            case InstrumentIdentity.GHLBass:
                return ref _ghlBass;
            case InstrumentIdentity.Keys;
                return ref _keys;
            case InstrumentIdentity.Vocals:
                return ref _vocals;
            default:
                throw new UndefinedEnumException(instrument);
        }
    }
}
