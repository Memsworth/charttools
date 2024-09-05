﻿namespace ChartTools.IO.Sources;

public class ReadingDataSource : DataSource
{
    public ReadingDataSource(Stream stream) : base(stream)
    {
        if (!stream.CanRead)
            throw new ArgumentException("Stream is unable to be read.", nameof(stream));
    }

    public ReadingDataSource(string path) : base(path, FileMode.Open, FileAccess.Read, FileShare.Write) { }

    public static implicit operator ReadingDataSource(Stream stream) => new(stream);

    public static implicit operator ReadingDataSource(string path) => new(path);
}
