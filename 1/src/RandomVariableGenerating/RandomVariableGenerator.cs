using System;
using System.Collections.Generic;
using Ardalis.GuardClauses;

namespace RandomVariableGenerating
{
    public class RandomVariableGenerator
    {
        private readonly Random _random;
        private readonly int[] _x;
        private readonly int[] _y;
        private readonly double[,] _probabilities;

        public RandomVariableGenerator(Random random, int[] x, int[] y, double[,] probabilities)
        {
            _random = random;
            _x = x;
            _y = y;
            _probabilities = probabilities;
        }

        public IEnumerable<(int x, int y)> Generate(int times)
        {
            Guard.Against.NegativeOrZero(times, nameof(times));
            
            var accumulativeDistributionFunctionF = BuildAccumulativeDistributionFunction(Buildfx());
            var normalizedAccumulativeDistributionFunctionF = BuildNormalizedAccumulativeDistributionFunction(BuildNormalizedR());
            for (var i = 0; i < times; i++)
            {
                var px = _random.NextDouble();
                var py = _random.NextDouble();
                
                var indexX = GetIndexX(px, accumulativeDistributionFunctionF);
                var indexY = GetIndexY(py, normalizedAccumulativeDistributionFunctionF, indexX);

                yield return (_x[indexX], _y[indexY]);
            }
        }

        private static double[] BuildAccumulativeDistributionFunction(double[] fx)
        {
            var len = fx.Length;
            var Fx = new double[len];
            
            var accumulator = fx[0];
            Fx[0] = accumulator;
            for (var i = 1; i < len; i++)
            {
                accumulator += fx[i];
                Fx[i] = accumulator;
            }

            return Fx;
        }

        private double[] Buildfx()
        {
            var rows = _probabilities.GetLength(0);
            var fx = new double[rows];
            var cols = _probabilities.GetLength(1);
            for (var i = 0; i < rows; i++)
            {
                var sum = 0d;
                for (var j = 0; j < cols; j++)
                    sum += _probabilities[i, j];
                fx[i] = sum;
            }

            return fx;
        }

        private double[,] BuildNormalizedAccumulativeDistributionFunction(double[,] normalizedR)
        {
            var rows = normalizedR.GetLength(0);
            var cols = normalizedR.GetLength(1);
            var normalized = new double[rows, cols];
            for (var i = 0; i < rows; i++)
            {
                var accumulator = normalizedR[i, 0];
                normalized[i, 0] = accumulator;
                for (var j = 1; j < cols; j++)
                {
                    accumulator += normalizedR[i, j];
                    normalized[i, j] = accumulator;
                }
            }

            return normalized;
        }

        private double[,] BuildNormalizedR()
        {
            var rows = _probabilities.GetLength(0);
            var cols = _probabilities.GetLength(1);
            var normalized = new double[rows, cols];
            for (var i = 0; i < rows; i++)
            {
                var norm = 0d;
                for (var j = 0; j < cols; j++)
                    norm += _probabilities[i, j];

                for (var j = 0; j < cols; j++)
                    normalized[i, j] = _probabilities[i, j] / norm;
            }

            return normalized;
        }

        private static int GetIndexX(double value, double[] Fx)
        {
            for (var i = 0; i < Fx.Length - 1; i++)
            {
                if (value > Fx[i] && value < Fx[i + 1])
                    return i + 1;
            }

            // it's should be impossible
            throw new InvalidOperationException(nameof(GetIndexX));
        }
        
        private static int GetIndexY(double value, double[,] normalizedF, int indexX)
        {
            var columnSize = normalizedF.GetLength(1);
            for (var i = 0; i < columnSize - 1; i++)
            {
                if (value > normalizedF[indexX, i] && value < normalizedF[indexX, i + 1])
                    return i + 1;
            }

            // it's should be impossible
            throw new InvalidOperationException(nameof(GetIndexY));
        }

        public static IEnumerable<(int x, int y)> Generate(Random random, int[] x, int[] y, double[,] probabilities, int times = 1)
        {
            return new RandomVariableGenerator(random, x, y, probabilities).Generate(times);
        }
    }
}
