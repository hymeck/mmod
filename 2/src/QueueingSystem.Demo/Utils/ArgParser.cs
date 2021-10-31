using System;
using System.Collections.Generic;
using QueueingSystem.Demo.Extensions;

namespace QueueingSystem.Demo.Utils
{
    public class ArgParser
    {
        private const double DefaultArrivalRate = 10d;
        private const double DefaultServiceRate = 3d;
        private const int DefaultServerCount = 3;
        private const int DefaultQueueCapacity = 4;
        private const double DefaultWaitingTime = 0.166666667d;

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

        public double GetArrivalRate() => _arrivalRate ??= _args.Count > 0
            ? _args[0].ParseDoubleOrDefault(DefaultArrivalRate)
            : DefaultArrivalRate;

        public double GetServiceRate() => _serviceRate ??= _args.Count > 1
            ? _args[1].ParseDoubleOrDefault(DefaultServiceRate)
            : DefaultServiceRate;

        public int GetServerCount() => _serverCount ??= _args.Count > 2
            ? int.TryParse(_args[2], out var serverCount)
                ? serverCount
                : DefaultServerCount
            : DefaultServerCount;

        public int GetQueueCapacity() => _queueCapacity ??= _args.Count > 3
            ? int.TryParse(_args[3], out var queueCapacity)
                ? queueCapacity
                : DefaultQueueCapacity
            : DefaultQueueCapacity;

        public double GetWaitingTime() => _waitingTime ??= _args.Count > 4
            ? _args[4].ParseDoubleOrDefault(DefaultWaitingTime)
            : DefaultWaitingTime;
    }
}
