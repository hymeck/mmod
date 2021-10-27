using System;
using System.Collections.Generic;
using System.Linq;
using Ardalis.GuardClauses;

namespace QueueingSystem
{
    public class QueueingSystemStatistics
    {
        private readonly QueueingSystemCharacteristics _characteristics;
        private double[] _probabilities;

        public QueueingSystemStatistics(QueueingSystemCharacteristics characteristics)
        {
            _characteristics = Guard.Against.Null(characteristics, nameof(characteristics));
        }

        public IReadOnlyList<double> Probabilities => _probabilities ??= BuildProbabilities();

        private double[] BuildProbabilities()
        {
            var n = _characteristics.ServerCount;
            var m = _characteristics.QueueCapacity;
            var trafficIntensity = _characteristics.ServiceRate / _characteristics.ArrivalRate;
            var trafficIntensitiesPowers = GetTrafficIntensityPowers(trafficIntensity, n, m);
            var probabilities = new double[n];

            return probabilities;
        }

        private static Dictionary<int, double> GetTrafficIntensityPowers(double trafficIntensity, int n, int m)
        {
            var count = Math.Max(n, m);
            var powers = Enumerable.Range(1, count)
                .Select(i => new KeyValuePair<int, double>(i, Math.Pow(trafficIntensity, i)));
            return new Dictionary<int, double>(powers);
        }
    }
}