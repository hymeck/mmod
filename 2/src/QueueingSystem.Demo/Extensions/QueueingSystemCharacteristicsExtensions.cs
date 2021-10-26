using System;
using System.Collections.Generic;
using QueueingSystem.Demo.Utils;

namespace QueueingSystem.Demo.Extensions
{
    public static class QueueingSystemCharacteristicsExtensions
    {
        public static QueueingSystemCharacteristics ToCharacteristics(this IReadOnlyList<string> args)
        {
            var p = new ArgParser(args);
            try
            {
                return new QueueingSystemCharacteristics(p.GetArrivalRate(), p.GetServiceRate(), p.GetServerCount(),
                    p.GetQueueCapacity(), p.GetWaitingTime());
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
