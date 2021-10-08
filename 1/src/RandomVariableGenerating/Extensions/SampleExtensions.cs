using System;
using System.Collections.Generic;
using Ardalis.GuardClauses;

namespace RandomVariableGenerating.Extensions
{
    public static class SampleExtensions
    {
        public static Histogram ToHistogram(this Sample sample)
        {
            var samplePoints = Guard.Against.Null(sample, nameof(sample)).SamplePoints;
            var volume = samplePoints.Count;
            var intervals = sample.IntervalCount;
            var itemsPerInterval = volume / intervals;

            double[] leftBounds = new double[intervals], // A
                rightBounds = new double[intervals], // B
                differences = new double[intervals], // h
                densityValues = new double[intervals]; // f*

            leftBounds[0] = samplePoints[0]; // A_0

            var factor = 1d / intervals; // in f*_i formula: v_i/n; v_i/n = (n/M)/ n = 1 / M
            for (var i = 1; i < intervals; i++) // i = 1..M - 1
            {
                var index = i * itemsPerInterval;
                var leftBound = (samplePoints[index - 1] + samplePoints[index]) / 2; // A_i = (x[(n/M) - 1] + x[n/M]) / 2

                var j = i - 1;
                rightBounds[j] = leftBounds[i] = leftBound; // B_i-1 = A_i
                differences[j] = leftBound - leftBounds[j]; // h_i-1 = B_i-1 - A_i-1
                densityValues[j] = factor / differences[j]; // f*_i-1 = 1 / (M * h_i-1)
            }

            intervals--; // set values to last elements of B, h, f* just for simplicity

            rightBounds[intervals] = samplePoints[volume - 1]; // B_last = x_last;
            differences[intervals] = rightBounds[intervals] - leftBounds[intervals]; // h_i_last
            densityValues[intervals] = factor / differences[intervals]; // f*_i_last

            return new Histogram(volume, intervals,
                leftBounds,
                rightBounds,
                differences,
                densityValues);
        }

        public static AccumulativeDistributionFunction ToDistributionFunction(this Sample sample)
        {
            var samplePoints = Guard.Against.Null(sample, nameof(sample)).SamplePoints;
            var occurrences = sample.Occurrences;
            var (relativeFrequencies, accumulatedFrequencies, totalCount, totalFrequency) = 
                BuildFrequencies(samplePoints, occurrences);
            return new AccumulativeDistributionFunction(
                occurrences,
                relativeFrequencies,
                accumulatedFrequencies,
                totalCount,
                totalFrequency);
        }

        private static (double[] relativeFrequencies, double[] accumulatedFrequencies, int totalCount, double totalFrequency) 
            BuildFrequencies(
            IReadOnlyList<double> samplePoints,
            IReadOnlyDictionary<double, int> occurrences)
        {
            var volume = samplePoints.Count;
            var relativeFrequencies = new double[volume];
            var accumulatedFrequencies = new double[volume];
            var i = 0;
            var totalCount = 0;
            var totalFrequency = 0d;
            foreach (var count in occurrences.Values)
            {
                var relativeFrequency = (double) count / volume;
                var accumulatedFrequency = relativeFrequency;

                if (i != 0)
                    accumulatedFrequency += accumulatedFrequencies[i - 1];

                relativeFrequencies[i] = relativeFrequency;
                accumulatedFrequencies[i] = accumulatedFrequency;

                totalCount += count;
                totalFrequency += relativeFrequency;
                i++;
            }

            accumulatedFrequencies[^1] = Math.Round(accumulatedFrequencies[^1]); // round last entry to 1
            return (relativeFrequencies, accumulatedFrequencies, totalCount, totalFrequency);
        }
    }
}
