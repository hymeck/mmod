using System.Globalization;

namespace SystemCharacteristics.Extensions
{
    public static class DoubleExtensions
    {
        public static string ToInvariantString(this double value) => value.ToString(CultureInfo.InvariantCulture);

        public static string Deviation(this double v1, double v2) =>
            v1.CompareTo(v2) switch
            {
                -1 => @"/\",
                0 => "=",
                1 => @"\/",
                _ => "undefined"
            };
    }
}
