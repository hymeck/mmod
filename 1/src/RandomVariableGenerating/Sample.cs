using System;
using System.Collections.Generic;
using System.Linq;
using RandomVariableGenerating.Extensions;

namespace RandomVariableGenerating
{
    public class Sample
    {
        public IReadOnlyList<double> SamplePoints { get; }
        public IReadOnlyDictionary<double, int> Occurrences { get; }

        public Sample(double[] source)
        {
            SamplePoints = source.ToSamplePoints();
            Occurrences = source.ToOccurrences();
        }

        public double SampleMean => SamplePoints.Sum() / SamplePoints.Count;
        
        // несмещенная состоятельная оценка дисперсии
        public double UnbiasedSampleVariance => CalculateSampleVariance();
        
        // смещенная состоятельная оценка дисперсии
        public double BiasedSampleVariance => CalculateSampleVariance(false);

        private double CalculateSampleVariance(bool isBiased = true)
        {
            var denominator = isBiased ? SamplePoints.Count : SamplePoints.Count - 1;
            return SamplePoints.Sum(point => Math.Pow(point - SampleMean, 2)) / denominator;
        }

        public int IntervalCount =>
            SamplePoints.Count <= 100
                ? (int) Math.Sqrt(SamplePoints.Count)
                : (int) (4 * Math.Log10(SamplePoints.Count));
    }
}
