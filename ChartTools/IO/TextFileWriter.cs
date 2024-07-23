using ChartTools.Extensions.Linq;
using ChartTools.Internal.Collections;
using ChartTools.IO.Sources;

namespace ChartTools.IO;

internal abstract class TextFileWriter(WritingDataSource source, IEnumerable<string>? removedHeaders, params Serializer<string>[] serializers)
{
    public WritingDataSource Source { get; } = source;

    protected virtual string? PreSerializerContent => null;
    protected virtual string? PostSerializerContent => null;

    private readonly List<Serializer<string>> serializers = [..serializers];
    private readonly IEnumerable<string>? removedHeaders = removedHeaders;

    private IEnumerable<SectionReplacement<string>> AddRemoveReplacements(IEnumerable<SectionReplacement<string>> replacements) => removedHeaders is null ? replacements : replacements.Concat(removedHeaders.Select(header => new SectionReplacement<string>(Enumerable.Empty<string>(), line => line == header, EndReplace, false)));

    private IEnumerable<string> Wrap(string header, IEnumerable<string> lines)
    {
        yield return header;

        if (PreSerializerContent is not null)
            yield return PreSerializerContent;

        foreach (var line in lines)
            yield return line;

        if (PostSerializerContent is not null)
            yield return PostSerializerContent;
    }

    public void Write()
    {
        using var writer = new StreamWriter(Source.Stream, leaveOpen: true);

        foreach (var line in GetLines(serializer => serializer.Serialize()))
            writer.WriteLine(line);

        EndFile();
    }

    public async Task WriteAsync(CancellationToken cancellationToken)
    {
        using var writer = new StreamWriter(Source.Stream, leaveOpen: true);

        foreach (var line in GetLines(serializer => new EagerEnumerable<string>(serializer.SerializeAsync())))
        {
            if (cancellationToken.IsCancellationRequested)
                break;

            await writer.WriteLineAsync(line);
        }

        EndFile();
    }

    private void EndFile() => Source.Stream.SetLength(Source.Stream.Position);

    private IEnumerable<string> GetLines(Func<Serializer<string>, IEnumerable<string>> getSerializerLines)
    {
        return Source.Existing is not null
            ? ReadExisting()
            .ReplaceSections(
                AddRemoveReplacements(from serializer in serializers select new SectionReplacement<string>(
                        Wrap(serializer.Header, getSerializerLines(serializer)),
                    line => line == serializer.Header, EndReplace, true)))
            : serializers.SelectMany(serializer => Wrap(serializer.Header, serializer.Serialize()));

        IEnumerable<string> ReadExisting()
        {
            using var reader = new StreamReader(Source.Existing.Stream, leaveOpen: true);

            var line = string.Empty;

            while ((line = reader.ReadLine()) != null)
                yield return line;
        }
    }

    protected abstract bool EndReplace(string line);
}
