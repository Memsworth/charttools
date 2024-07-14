using ChartTools.IO.Configuration;
using ChartTools.IO.Formatting;

namespace ChartTools.IO.Chart.Configuration.Sessions;

internal abstract class ChartSession(ComponentList components, FormattingRules? formatting) : Session(components, formatting)
{
    public override abstract CommonChartConfiguration Configuration { get; }
}
