namespace ChartTools.IO.Sources;

internal abstract class DataSource(Stream stream) : IDisposable
{
    public string? Path { get; }
    public Stream Stream { get; } = stream;

    private readonly bool _disposeStream = false;

    public DataSource(string path, FileMode mode, FileAccess access) : this(new FileStream(path, mode, access))
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
