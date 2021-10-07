using System.Collections.Generic;

namespace RandomVariableGenerating
{
    public record Histogram(
        int Volume, int IntervalCount,
        IReadOnlyList<double> LeftBounds,
        IReadOnlyList<double> RightBounds,
        IReadOnlyList<double> Differences,
        IReadOnlyList<double> DensityValues)
    {
        private int? _itemsPerInterval;
        public int ItemsPerInterval => _itemsPerInterval ??= Volume / IntervalCount;
    }
}
