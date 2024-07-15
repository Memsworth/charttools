using ChartTools.IO.Parsing;

namespace ChartTools.IO.Ini;

internal class IniFileReader(string path, Metadata? existing) : TextFileReader(path)
{
    public override IEnumerable<IniParser> Parsers => base.Parsers.Cast<IniParser>();

    protected override TextParser? GetParser(string header) => header.Equals(IniFormatting.Header, StringComparison.OrdinalIgnoreCase) ? new IniParser(existing) : null;
    public IniFileReader(TextReader reader, Func<string, IniParser?> parserGetter) : base(reader, parserGetter) { }
    public IniFileReader(Stream stream, Func<string, IniParser?> parserGetter) : base(stream, parserGetter) { }
    public IniFileReader(string path, Func<string, IniParser?> parserGetter) : base(path, parserGetter) { }

    protected override bool IsSectionStart(string line) => !line.StartsWith('[');
}
