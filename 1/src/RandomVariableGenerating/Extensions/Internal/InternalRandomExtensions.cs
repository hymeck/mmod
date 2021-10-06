using System;

namespace RandomVariableGenerating.Extensions.Internal
{
    internal static class InternalRandomExtensions
    {
        public static double GenerateLessThan(this Random random, double upperBound, double precision = 1e-6)
        {
            generateValue:
            var value = random.NextDouble();
            var diff = upperBound - value;
            if (diff > 0)
                return value;
            goto generateValue;
        }
    }
}
