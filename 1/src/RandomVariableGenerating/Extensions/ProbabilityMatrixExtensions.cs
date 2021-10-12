using System;
using System.Collections.Generic;
using System.Linq;
using Ardalis.GuardClauses;
using MathNet.Numerics.LinearAlgebra.Double;

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

        public static double Correlation(this ProbabilityMatrix matrix, IReadOnlyList<int> sourceX, IReadOnlyList<int> sourceY)
        {
            var meanX = matrix.MeanXPointEstimation(sourceX);
            var meanY = matrix.MeanYPointEstimation(sourceY);
            var varianceX = Variance(matrix.XProbabilities, meanX, sourceX);
            var varianceY = Variance(matrix.YProbabilities, meanY, sourceY);
            var meanXy = MeanXY(matrix, sourceX, sourceY);
            return (meanXy - (meanX * meanY)) / Math.Sqrt(varianceX * varianceY);
        }

        public static double ChiSquared(this ProbabilityMatrix empiricalMatrix, ProbabilityMatrix matrix,
            IReadOnlyList<int> sourceX, IReadOnlyList<int> sourceY, int volume)
        {
            Guard.Against.Null(empiricalMatrix, nameof(empiricalMatrix));
            Guard.Against.Null(matrix, nameof(matrix));
            Guard.Against.Null(sourceX, nameof(sourceX));
            Guard.Against.Null(sourceY, nameof(sourceY));
            Guard.Against.NegativeOrZero(volume, nameof(volume));

            var left = DenseMatrix.Build.DenseOfArray(empiricalMatrix);
            var right = DenseMatrix.Build.DenseOfArray(matrix);
            var squared = (left - right).Map(item => Math.Pow(item, 2));
            var division = squared.PointwiseDivide(right);
            return division.Enumerate().Sum() * volume;
        }

        private static double MeanXY(double[,] probabilities, IReadOnlyList<int> sourceX, IReadOnlyList<int> sourceY)
        {
            var matrix = DenseMatrix.Build.DenseOfArray(probabilities).Transpose();
            var column = Vector.Build.DenseOfEnumerable(sourceX.Select(x => (double) x)).ToColumnMatrix();
            var row = Vector.Build.DenseOfEnumerable(sourceY.Select(x => (double) x)).ToColumnMatrix();
            var product = (matrix * column).Transpose();
            var result = product * row;
            return result.Enumerate().Sum();
        }

        private static double MeanPointEstimation(IReadOnlyList<double> probabilities, IReadOnlyCollection<int> source) =>
            source
                .Select((item, index) => item * probabilities[index])
                .Sum();

        private static double Variance(IReadOnlyList<double> probabilities, double mean, IReadOnlyCollection<int> source) =>
            source
                .Select((item, index) => probabilities[index] * Math.Pow(item - mean, 2))
                .Sum();
    }
}
