using ChartTools.IO;
using ChartTools.IO.Chart;

namespace ChartTools.Events;

/// <summary>
/// Provides additional methods for events.
/// </summary>
public static class EventExtensions
{
    [Obsolete]
    public static void ToFile(this IEnumerable<GlobalEvent> events, string path) => ExtensionHandler.Write(path, events, (".chart", ChartFile.ReplaceGlobalEvents));

    [Obsolete]
    public static async Task ToFileAsync(this IEnumerable<GlobalEvent> events, string path, CancellationToken cancellationToken) => await ExtensionHandler.WriteAsync(path, events, (".chart", (path, events) => ChartFile.ReplaceGlobalEventsAsync(path, events, cancellationToken)));
}
