using System;
using MathNet.Numerics.Distributions;

namespace RandomVariableGenerating.Extensions
{
    public static class DoubleExtensions
    {
        public static double Student(this double significance)
        {
            if (significance < 0 || significance > 1)
                throw new ArgumentException("significance should be from 0 to 1 (inclusive).", nameof(significance));
            return new Normal().InverseCumulativeDistribution((significance + 1) / 2);
        }
    }
}
