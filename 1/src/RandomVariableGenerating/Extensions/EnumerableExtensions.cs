using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Ardalis.GuardClauses;

namespace RandomVariableGenerating.Extensions
{
    public static class EnumerableExtensions
    {
        public static IReadOnlyDictionary<T, int> ToOccurrences<T>(this IEnumerable<T> source) =>
            source.SourceOrThrowIfNull(nameof(source))
                .InternalToOccurrences()
                .ToImmutableDictionary();

        public static IReadOnlyList<T> ToSamplePoints<T>(this IEnumerable<T> source) =>
            source.SourceOrThrowIfNull(nameof(source))
                .InternalToSamplePoints()
                .ToImmutableList();

        public static IReadOnlyList<double> ToSamplePoints(this IEnumerable<double> source, int roundDigits = 1) =>
            source.SourceOrThrowIfNull(nameof(source))
                .Select(item => Math.Round(item, Guard.Against.NegativeOrZero(roundDigits, nameof(roundDigits))))
                .InternalToSamplePoints()
                .ToImmutableList();

        private static IEnumerable<T> SourceOrThrowIfNull<T>(this IEnumerable<T> source, string parameterName) =>
            Guard.Against.Null(source, parameterName);

        private static IEnumerable<KeyValuePair<T, int>> InternalToOccurrences<T>(this IEnumerable<T> samplePoints) =>
            samplePoints
                .GroupBy(item => item)
                .Select(group => new KeyValuePair<T, int>(group.Key, group.Count()));

        private static IEnumerable<T> InternalToSamplePoints<T>(this IEnumerable<T> source) =>
            source.OrderBy(item => item);
    }
}
