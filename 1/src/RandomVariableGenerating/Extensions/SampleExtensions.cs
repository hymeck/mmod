using System;
using Ardalis.GuardClauses;

namespace RandomVariableGenerating.Extensions
{
    public static class SampleExtensions
    {
        public static (double leftBound, double rightBound) IntervalEstimation(this Sample sample, double significance)
        {
            Guard.Against.Null(sample, nameof(sample));
            var studentFactor = significance.Student();
            var dividedVariance = sample.UnbiasedSampleVariance / sample.Volume;
            var delta = Math.Sqrt(dividedVariance) * studentFactor;
            var left = sample.SampleMean - delta;
            var right = sample.SampleMean + delta;
            return (left, right);
        }
    }
}
