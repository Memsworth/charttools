namespace ChartTools.IO.Ini;

internal class IniFileWriter
    : TextFileWriter
{
    public IniFileWriter(TextWriter writer, params Serializer<string>[] serializers) : base(writer, Enumerable.Empty<string>(), serializers) { }
    public IniFileWriter(Stream stream, params Serializer<string>[] serializers) : base(stream, Enumerable.Empty<string>(), serializers) { }
    public IniFileWriter(string path, params Serializer<string>[] serializers) : base(path, Enumerable.Empty<string>(), serializers) { }

    protected override bool EndReplace(string line) => line.StartsWith('[');
}
