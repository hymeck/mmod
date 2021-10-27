using Ardalis.GuardClauses;

namespace QueueingSystem.Extensions
{
    public static class QueueingSystemCharacteristicsExtensions
    {
        public static double TrafficIntensity(this QueueingSystemCharacteristics characteristics) => 
            InternalTrafficIntensity(Guard.Against.Null(characteristics, nameof(characteristics)));

        internal static double InternalTrafficIntensity(this QueueingSystemCharacteristics characteristics) =>
            characteristics.ArrivalRate / characteristics.ServiceRate;
    }
}
