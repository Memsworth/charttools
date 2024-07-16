namespace ChartTools.IO.Sources.Text;

internal class TextReadDataSource : DataSource
{
    public TextReader Reader { get; }

    private readonly bool _disposeReader = false;

    public TextReadDataSource(TextReader reader)
        => Reader = reader;

    public TextReadDataSource(Stream stream) : base(stream)
    {
        Reader = new StreamReader(stream, leaveOpen: true);
        _disposeReader = true;
    }

    public TextReadDataSource(string path) : base(path)
    {
        Reader = new StreamReader(Stream!);
        _disposeReader = true;
    }

    public override void Dispose()
    {
        if (_disposeReader)
            Reader.Dispose();
    }
}
