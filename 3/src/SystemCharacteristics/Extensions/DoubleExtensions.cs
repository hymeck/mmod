using System.Globalization;

namespace SystemCharacteristics.Extensions
{
    public static class DoubleExtensions
    {
        public static string ToInvariantString(this double value) => value.ToString(CultureInfo.InvariantCulture);
    }
}