namespace ChartTools.IO.Sources;

internal class DataSource : IDisposable
{
    public string? Path { get; }
    public Stream? Stream { get; }

    private readonly bool _disposeStream = false;

    // Invokable base for derived sources not using a path or stream. Should be exposed publicly with this signature - Data required
    protected DataSource() { }

    public DataSource(Stream stream) => Stream = stream;

    public DataSource(string path) : this(new FileStream(path, FileMode.Open, FileAccess.Read))
    {
        Path = path;
        _disposeStream = true;
    }

    public virtual void Dispose()
    {
        if (_disposeStream)
            Stream!.Dispose();
    }
}
