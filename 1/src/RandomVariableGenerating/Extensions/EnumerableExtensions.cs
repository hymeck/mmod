using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace RandomVariableGenerating.Extensions
{
    public static class EnumerableExtensions
    {
        public static IReadOnlyList<T> ToSamplePoints<T>(this IEnumerable<T> source) =>
            source
                .InternalToSamplePoints()
                .ToImmutableList();

        private static IEnumerable<T> InternalToSamplePoints<T>(this IEnumerable<T> source) =>
            source.OrderBy(item => item);
    }
}
