using System.Collections.Generic;

namespace RandomVariableGenerating
{
    public record AccumulativeDistributionFunction(IReadOnlyDictionary<double, int> PointOccurrences,
        IReadOnlyList<double> RelativeFrequencies,
        IReadOnlyList<double> AccumulatedFrequencies,
        int TotalCount,
        double TotalFrequency);
}
