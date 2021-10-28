using System;
using System.Collections.Generic;
using System.Linq;
using Ardalis.GuardClauses;
using QueueingSystem.Extensions;
// ReSharper disable PossibleMultipleEnumeration

namespace QueueingSystem.Utils
{
    internal class QueueingSystemCharacteristicsHelper
    {
        private readonly Lazy<Dictionary<int, double>> _trafficIntensitiesPowers;
        private readonly Lazy<Dictionary<int, double>> _factorials;
        private readonly Lazy<Dictionary<int, double>> _factors;
        private readonly Lazy<IReadOnlyList<double>> _probabilities;

        public QueueingSystemCharacteristicsHelper(QueueingSystemCharacteristics characteristics)
        {
            Characteristics = Guard.Against.Null(characteristics, nameof(characteristics));
            _trafficIntensitiesPowers = new Lazy<Dictionary<int, double>>(GetTrafficIntensitiesPowers);
            _factorials = new Lazy<Dictionary<int, double>>(GetFactorials);
            _factors = new Lazy<Dictionary<int, double>>(GetFactors);
            _probabilities = new Lazy<IReadOnlyList<double>>(GetProbabilities);
        }

        public IReadOnlyList<double> Probabilities => _probabilities.Value;
        public QueueingSystemCharacteristics Characteristics { get; }

        private IReadOnlyList<double> GetProbabilities()
        {
            var trafficIntensitiesPowers = _trafficIntensitiesPowers.Value;
            var factorials = _factorials.Value;
            var factors = _factors.Value;

            var n = Characteristics.ServerCount;
            var m = Characteristics.QueueCapacity;
            
            var nProbabilitiesFactors = trafficIntensitiesPowers
                .Take(n)
                .Zip(factorials)
                .Select(p => p.First.Value / p.Second.Value);
            var sum1 = nProbabilitiesFactors.Sum();

            var pnFactor = trafficIntensitiesPowers[n] / factorials[n];

            var mProbabilitiesFactors = trafficIntensitiesPowers
                .Take(m)
                .Zip(factors)
                .Select(p => p.First.Value / p.Second.Value);
            var sum2 = mProbabilitiesFactors.Sum();
            
            var p0 = 1d / (1d + sum1 + pnFactor * sum2);
            var pn = p0 * pnFactor;

            var nProbabilities = nProbabilitiesFactors.Select(f => p0 * f);
            var mProbabilities = mProbabilitiesFactors.Select(f => f * pn);

            return nProbabilities
                .Prepend(p0)
                .Concat(mProbabilities)
                .ToArray();
        }

        private Dictionary<int, double> GetTrafficIntensitiesPowers()
        {
            var n = Characteristics.ServerCount;
            var m = Characteristics.QueueCapacity;
            var trafficIntensity = Characteristics.InternalTrafficIntensity();
            var count = Math.Max(n, m);
            var powers = Enumerable
                .Range(1, count)
                .Select(i => new KeyValuePair<int, double>(i, Math.Pow(trafficIntensity, i)));
            return new Dictionary<int, double>(powers);
        }

        private Dictionary<int, double> GetFactorials()
        {
            var n = Characteristics.ServerCount;
            var factorials = Enumerable
                .Range(1, n)
                .Select(k => Enumerable.Range(1, k).Aggregate(1, (f, i) => f * i))
                .Select((factorial, i) => new KeyValuePair<int, double>(i + 1, factorial));
            return new Dictionary<int, double>(factorials);
        }

        private Dictionary<int, double> GetFactors()
        {
            var n = Characteristics.ServerCount;
            var m = Characteristics.QueueCapacity;
            var beta = 1 / (Characteristics.WaitingTime * Characteristics.ServiceRate);
            var factors = Enumerable
                .Range(1, m)
                .Select(i => Enumerable.Range(1, i).Aggregate(1d, (total, l) => total * (n + l * beta)))
                .Select((factor, index) => new KeyValuePair<int, double>(index + 1, factor));
            return new Dictionary<int, double>(factors);
        }
    }
}
