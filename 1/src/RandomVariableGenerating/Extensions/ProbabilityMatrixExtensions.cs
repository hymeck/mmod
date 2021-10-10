using System;
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

        public static double VarianceXPointEstimation(this ProbabilityMatrix matrix, IReadOnlyList<int> source) =>
            Variance(Guard.Against.Null(matrix, nameof(matrix)).XProbabilities, matrix.MeanXPointEstimation(source),
                source);
        
        public static double VarianceYPointEstimation(this ProbabilityMatrix matrix, IReadOnlyList<int> source) =>
            Variance(Guard.Against.Null(matrix, nameof(matrix)).YProbabilities, matrix.MeanYPointEstimation(source),
                source);

        private static double MeanPointEstimation(IReadOnlyList<double> probabilities, IReadOnlyCollection<int> source) =>
            Mean(probabilities, source);

        private static double Mean(IReadOnlyList<double> probabilities, IReadOnlyCollection<int> source) =>
            source
                .Select((item, index) => item * probabilities[index])
                .Sum();

        private static double Variance(IReadOnlyList<double> probabilities, double mean, IReadOnlyCollection<int> source) =>
            source
                .Select((item, index) => probabilities[index] * Math.Pow(item - mean, 2))
                .Sum();
    }
}
