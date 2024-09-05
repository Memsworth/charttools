﻿using ChartTools.IO.Sources;

namespace ChartTools.IO.Chart;

internal class ChartFileWriter(WritingDataSource source, IEnumerable<string>? removedHeaders, params Serializer<string>[] serializers)
    : TextFileWriter(source, removedHeaders, serializers), IDisposable
{


    protected override string? PreSerializerContent => "{";
    protected override string? PostSerializerContent => "}";

    protected override bool EndReplace(string line) => line.StartsWith('[');

    public void Dispose() => Source.Dispose();
}
