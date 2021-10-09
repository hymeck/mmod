using System.Collections.Generic;
using System.Linq;
using Ardalis.GuardClauses;

namespace RandomVariableGenerating.Extensions
{
    public static class ProbabilityMatrixExtensions
    {
        public static double MeanXPointEstimation(this ProbabilityMatrix matrix, IReadOnlyList<int> source) =>
            MeanPointEstimation(Guard.Against.Null(matrix, nameof(matrix)).XProbabilities,
                Guard.Against.Null(source, nameof(source)));

        public static double MeanYPointEstimation(this ProbabilityMatrix matrix, IReadOnlyList<int> source) =>
            MeanPointEstimation(Guard.Against.Null(matrix, nameof(matrix)).YProbabilities,
                Guard.Against.Null(source, nameof(source)));

        private static double MeanPointEstimation(IReadOnlyList<double> probabilities, IReadOnlyCollection<int> source) =>
            Mean(probabilities, source) / source.Count; // 

        private static double Mean(IReadOnlyList<double> probabilities, IReadOnlyCollection<int> source) =>
            source
                .Select((item, index) => item * probabilities[index])
                .Sum();
    }
}
