using ChartTools.Extensions.Collections;
using ChartTools.IO.Parsing;

namespace ChartTools.IO;

internal abstract class TextFileReader(string path) : FileReader<string, TextParser>(path)
{
    public virtual bool DefinedSectionEnd { get; } = false;

    protected override void ReadBase(bool async, CancellationToken cancellationToken)
    {
        ParserContentGroup? currentGroup = null;
        using var enumerator = File.ReadLines(Path).Where(s => !string.IsNullOrEmpty(s)).Select(s => s.Trim()).GetEnumerator();

        while (enumerator.MoveNext()) // Runs once for every section
        {
            currentGroup = null;

            // Find section
            while (!enumerator.Current.StartsWith('['))
                if (enumerator.MoveNext())
                    return;

            if (async && cancellationToken.IsCancellationRequested)
            {
                Dispose();
                return;
            }

            var header = enumerator.Current;
            var parser = GetParser(header);

            if (parser is not null)
            {
                var source = new DelayedEnumerableSource<string>();

                parserGroups.Add(currentGroup = new(parser, source));

                if (async)
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        Dispose();
                        return;
                    }

                    parseTasks.Add(parser.StartAsyncParse(source.Enumerable));
                }
            }

            // Move to the start of the entries
            do
                if (!AdvanceSection())
                {
                    FinishSection();
                    return;
                }
            while (!IsSectionStart(enumerator.Current));

            AdvanceSection();

            // Read until end
            while (!IsSectionEnd(enumerator.Current))
            {
                currentGroup?.Source.Add(enumerator.Current);

                if (!AdvanceSection())
                {
                    FinishSection();
                    return;
                }
            }

            FinishSection();

            void FinishSection()
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    Dispose();
                    return;
                }

                currentGroup?.Source.EndAwait();
            }

            bool AdvanceSection() => enumerator.MoveNext() || (DefinedSectionEnd ? throw SectionException.EarlyEnd(header) : false);
        }
    }

    protected abstract bool IsSectionStart(string line);
    protected virtual bool IsSectionEnd(string line) => false;
}
