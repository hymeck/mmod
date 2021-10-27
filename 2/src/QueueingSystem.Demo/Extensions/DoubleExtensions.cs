using System.Globalization;

namespace QueueingSystem.Demo.Extensions
{
    public static class DoubleExtensions
    {
        public static string ToInvariantCultureString(this double value) => value.ToString(CultureInfo.InvariantCulture);
    }
}
