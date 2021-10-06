using System;
using Ardalis.GuardClauses;
using RandomVariableGenerating.Extensions;
using RandomVariableGenerating.Extensions.Internal;

namespace RandomVariableGenerating.Utils
{
    public class ProbabilityMatrixGenerator
    {
        public const int DefaultDigits = 6;
        public const double DefaultPrecision = 1e-6;
        
        private readonly Random _random;
        private readonly int _rows;
        private readonly int _columns;
        private readonly int _digits;
        private readonly double _precision;

        public ProbabilityMatrixGenerator(Random random, int rows, int columns, int digits = DefaultDigits, double precision = DefaultPrecision)
        {
            _random = Guard.Against.Null(random, nameof(random));
            _rows = Guard.Against.NegativeOrZero(rows, nameof(rows));
            _columns = Guard.Against.NegativeOrZero(columns, nameof(columns));
            _digits = Guard.Against.Negative(digits, nameof(digits));
            _precision = Guard.Against.Negative(precision, nameof(precision));
        }

        public double[,] BuildProbabilityMatrix()
        {
            var probabilities = new double[_rows, _columns];
            var accumulator = 0d;
            for (var i = 0; i < _rows; i++)
            {
                SetRow(i, ref accumulator, ref probabilities);
            }

            return probabilities;
        }

        private void SetRow(int currentRow, ref double accumulator, ref double[,] probabilities)
        {
            for (var j = 0; j < _columns; j++)
            {
                var p = GetRoundedProbability(accumulator);
                accumulator += p;
                probabilities[currentRow, j] = p;
            }
        }

        private double GetRoundedProbability(double accumulator) =>
            Math.Round(_random.GenerateLessThan(1d - accumulator, _precision), _digits);
        
        public static double[,] BuildProbabilityMatrix(Random random, int rows, int columns, int digits = DefaultDigits) =>
            new ProbabilityMatrixGenerator(random, rows, columns, digits).BuildProbabilityMatrix();
    }
}
