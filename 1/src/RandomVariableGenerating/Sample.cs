using System;
using System.Collections.Generic;
using System.Linq;
using RandomVariableGenerating.Extensions;

namespace RandomVariableGenerating
{
    public class Sample
    {
        private double? _sampleMean;
        private double? _unbiasedSampleVariance;

        public Sample(IEnumerable<double> source)
        {
            SamplePoints = source.ToSamplePoints();
        }
        
        public IReadOnlyList<double> SamplePoints { get; }

        public int Volume => SamplePoints.Count;
        public double SampleMean => _sampleMean ??= SamplePoints.Sum() / Volume;
        public double UnbiasedSampleVariance => _unbiasedSampleVariance ??= CalculateSampleVariance(false);

        private double CalculateSampleVariance(bool isBiased = true)
        {
            var denominator = isBiased ? Volume : Volume - 1;
            return SamplePoints.Sum(point => Math.Pow(point - SampleMean, 2)) / denominator;
        }
    }
}
