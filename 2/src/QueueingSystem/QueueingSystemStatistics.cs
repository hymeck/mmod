using System.Collections.Generic;
using Ardalis.GuardClauses;
using QueueingSystem.Extensions.Internal;

namespace QueueingSystem
{
    public class QueueingSystemStatistics
    {
        private readonly QueueingSystemCharacteristicsHelper _helper;

        public QueueingSystemStatistics(QueueingSystemCharacteristics characteristics)
        {
            _helper = new QueueingSystemCharacteristicsHelper(Guard.Against.Null(characteristics, nameof(characteristics)));
        }

        public IReadOnlyList<double> Probabilities => _helper.Probabilities;
        public QueueingSystemCharacteristics Characteristics => _helper.Characteristics;
    }
}
