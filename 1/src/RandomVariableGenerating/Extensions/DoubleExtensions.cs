using Ardalis.GuardClauses;
using MathNet.Numerics.Distributions;

namespace RandomVariableGenerating.Extensions
{
    public static class DoubleExtensions
    {
        public static double Student(this double significance) => 
            new Normal().InverseCumulativeDistribution((Guard.Against.OutOfProbability(significance) + 1) / 2);

        public static double ChiSquared(this double confidence, double freedom) =>
            new ChiSquared(freedom).InverseCumulativeDistribution(1 - Guard.Against.OutOfProbability(confidence));
    }
}
