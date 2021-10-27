using System.Globalization;

namespace QueueingSystem.Extensions.Internal
{
    public static class StringExtensions
    {
        public static double ParseDoubleOrDefault(this string source, double defaultValue) =>
            double.TryParse(source, NumberStyles.Any, CultureInfo.InvariantCulture, out var value)
                ? value
                : defaultValue;
    }
}
