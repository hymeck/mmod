using System;
using System.Collections.Generic;

namespace QueueingSystem.Demo.Utils
{
    public class ArgParser
    {
        private const double DefaultArrivalRate = 1d;
        private const double DefaultServiceRate = 2d;
        private const int DefaultServerCount = 1;
        private const int DefaultQueueCapacity = 1;
        private const double DefaultWaitingTime = double.PositiveInfinity;
        
        private readonly IReadOnlyList<string> _args;
        private double? _arrivalRate;
        private double? _serviceRate;
        private int? _serverCount;
        private int? _queueCapacity;
        private double? _waitingTime;

        public ArgParser(IReadOnlyList<string> args)
        {
            _args = args ?? Array.Empty<string>();
        }

        public double GetArrivalRate() => _arrivalRate ??= _args.Count == 1
            ? double.TryParse(_args[0], out var arrivalRate)
                ? arrivalRate
                : DefaultArrivalRate
            : DefaultArrivalRate;

        public double GetServiceRate() => _serviceRate ??= _args.Count == 2
            ? int.TryParse(_args[1], out var serviceRate)
                ? serviceRate
                : DefaultServiceRate
            : DefaultServiceRate;

        public int GetServerCount() => _serverCount ??= _args.Count == 3
            ? int.TryParse(_args[2], out var serverCount)
                ? serverCount
                : DefaultServerCount
            : DefaultServerCount;

        public int GetQueueCapacity() => _queueCapacity ??= _args.Count == 4
            ? int.TryParse(_args[3], out var queueCapacity)
                ? queueCapacity
                : DefaultQueueCapacity
            : DefaultQueueCapacity;
        
        public double GetWaitingTime() => _waitingTime ??= _args.Count == 5
            ? double.TryParse(_args[4], out var waitingTime)
                ? waitingTime
                : DefaultWaitingTime
            : DefaultWaitingTime;
    }
}
