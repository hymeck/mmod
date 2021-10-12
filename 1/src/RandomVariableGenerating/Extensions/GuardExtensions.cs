using System;
using Ardalis.GuardClauses;

namespace RandomVariableGenerating.Extensions
{
    public static class GuardExtensions
    {
        public static double OutOfProbability(this IGuardClause guard, double value)
        {
            if (value is < 0 or > 1)
                throw new ArgumentException("significance should be from 0 to 1 (inclusive).", nameof(value));
            return value;
        }
    }
}
