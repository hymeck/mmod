using System.Collections.Generic;
using System.Linq;
using Ardalis.GuardClauses;
using QueueingSystem.Extensions;
using QueueingSystem.Utils;

namespace QueueingSystem
{
    public class QueueingSystemStatistics
    {
        private readonly QueueingSystemCharacteristicsHelper _helper;
        private double? _averageCustomersInQueue;
        private double? _averageCustomersInSystem;

        public QueueingSystemStatistics(QueueingSystemCharacteristics characteristics)
        {
            _helper = new QueueingSystemCharacteristicsHelper(Guard.Against.Null(characteristics,
                nameof(characteristics)));
        }

        public QueueingSystemCharacteristics Characteristics => _helper.Characteristics;
        public IReadOnlyList<double> Probabilities => _helper.Probabilities;
        public double RejectionProbability => Probabilities[^1];
        public double RelativeBandwidth => 1 - RejectionProbability;
        public double AbsoluteBandwidth => Characteristics.ArrivalRate * RelativeBandwidth;

        public double AverageCustomersInQueue => _averageCustomersInQueue ??= Probabilities
            .Skip(Characteristics.ServerCount + 1)
            .Select((p, i) => p * (i + 1))
            .Sum();

        public double AverageCustomersInSystem => _averageCustomersInSystem ??=
            Probabilities
                .Skip(1)
                .Take(Characteristics.ServerCount)
                .Select((p, i) => p * (i + 1))
                .Sum() + 
            Probabilities
                .TakeLast(Characteristics.QueueCapacity)
                .Select(p => Characteristics.ServerCount * p)
                .Sum();

        public double AverageTimeInQueue => RelativeBandwidth / Characteristics.ServiceRate;
        public double AverageTimeInSystem => AverageCustomersInSystem / Characteristics.ArrivalRate;
        public double AverageBusyChannels => RelativeBandwidth * Characteristics.InternalTrafficIntensity();
    }
}
