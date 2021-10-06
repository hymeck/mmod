using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Ardalis.GuardClauses;

namespace RandomVariableGenerating.Extensions
{
    public static class EnumerableExtensions
    {
        public static IReadOnlyDictionary<T, int> ToOccurrences<T>(this IEnumerable<T> samplePoints) =>
            Guard.Against.Null(samplePoints, nameof(samplePoints))
                .GroupBy(item => item)
                .Select(group => new KeyValuePair<T, int>(group.Key, group.Count()))
                .ToImmutableDictionary();

        public static IReadOnlyList<T> ToSamplePoints<T>(this IEnumerable<T> source) =>
            Guard.Against.Null(source, nameof(source))
                .OrderBy(item => item)
                .ToImmutableList();
    }
}
