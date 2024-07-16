namespace ChartTools.IO.Sources.Text;

internal class TextWriteDataSource : DataSource
{
    public TextWriter Writer { get; }

    private readonly bool _disposeWriter = false;

    public TextWriteDataSource(TextWriter writer)
        => Writer = writer;

    public TextWriteDataSource(Stream stream) : base(stream)
    {
        Writer = new StreamWriter(stream, leaveOpen: true);
        _disposeWriter = true;
    }

    public TextWriteDataSource(string path) : base(path)
    {
        Writer = new StreamWriter(Stream!);
        _disposeWriter = true;
    }

    public override void Dispose()
    {
        if (_disposeWriter)
            Writer.Dispose();
    }
}
