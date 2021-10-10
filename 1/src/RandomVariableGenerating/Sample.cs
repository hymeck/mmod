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
        private double? _biasedSampleVariance;
        private int? _intervalCount;

        public Sample(double[] source, int sampleRoundDigits = 1)
        {
            SamplePoints = source.ToSamplePoints(sampleRoundDigits);
            Occurrences = SamplePoints.ToOccurrences();
        }

        public Sample(IEnumerable<double> source)
        {
            SamplePoints = source.ToSamplePoints();
            Occurrences = SamplePoints.ToOccurrences();
        }
        
        public IReadOnlyList<double> SamplePoints { get; }
        public IReadOnlyDictionary<double, int> Occurrences { get; }

        public int Volume => SamplePoints.Count;
        public double SampleMean => _sampleMean ??= SamplePoints.Sum() / Volume;
        
        // несмещенная состоятельная оценка дисперсии
        public double UnbiasedSampleVariance => _unbiasedSampleVariance ??= CalculateSampleVariance(false);
        
        // смещенная состоятельная оценка дисперсии
        public double BiasedSampleVariance => _biasedSampleVariance ??= CalculateSampleVariance();

        private double CalculateSampleVariance(bool isBiased = true)
        {
            var denominator = isBiased ? Volume : Volume - 1;
            return SamplePoints.Sum(point => Math.Pow(point - SampleMean, 2)) / denominator;
        }

        public int IntervalCount => _intervalCount ??=
            SamplePoints.Count <= 100
                ? (int) Math.Sqrt(SamplePoints.Count)
                : (int) (4 * Math.Log10(SamplePoints.Count));
    }
}
