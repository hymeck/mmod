using System;
using System.Linq;
using QueueingSystem.Demo.Extensions;
using static System.Console;

namespace QueueingSystem.Demo
{
    class QueueingSystemProgram
    {
        private static void Main(string[] args)
        {
            var characteristics = args.ToCharacteristics();
            if (characteristics == null)
            {
                WriteLine("At least one argument is not valid. Try again.");
                PrintHelp();
                Environment.Exit(1);
            }

            var statistics = new QueueingSystemStatistics(characteristics);
            PrintCharacteristics(statistics.Characteristics);
            WriteLine();
            PrintStatistics(statistics);
            WriteLine();
        }

        private static void PrintHelp()
        {
            WriteLine("Command arguments: ar sr sc qc wt");
            WriteLine("ar - arrival rate");
            WriteLine("sr - service rate");
            WriteLine("sc - server count");
            WriteLine("qc - queue capacity");
            WriteLine("wt - waiting time");
        }

        private static void PrintCharacteristics(QueueingSystemCharacteristics characteristics)
        {
            WriteLine($"Arrival rate: {characteristics.ArrivalRate.ToInvariantCultureString()}");
            WriteLine($"Service rate: {characteristics.ServiceRate.ToInvariantCultureString()}");
            WriteLine($"Server count: {characteristics.ServerCount.ToString()}");
            WriteLine($"Queue capacity: {characteristics.QueueCapacity.ToString()}");
            WriteLine($"Waiting time: {characteristics.WaitingTime.ToInvariantCultureString()}");
        }

        private static void PrintStatistics(QueueingSystemStatistics statistics)
        {
            var probabilitiesStrings = statistics.Probabilities
                .Select((p, i) => $"#{i.ToString()}: {p.ToInvariantCultureString()}");
            WriteLine("Probabilities: ");
            WriteLine(string.Join(Environment.NewLine, probabilitiesStrings));
            WriteLine($"Sum: {statistics.Probabilities.Sum().ToInvariantCultureString()}");
            WriteLine();
            WriteLine($"Probability of Rejection: {statistics.RejectionProbability.ToInvariantCultureString()}");
            WriteLine($"Relative bandwidth: {statistics.RelativeBandwidth.ToInvariantCultureString()}");
            WriteLine($"Absolute bandwidth: {statistics.AbsoluteBandwidth.ToInvariantCultureString()}");
            WriteLine($"Average number of customers in queue: {statistics.AverageCustomersInQueue.ToInvariantCultureString()}");
            WriteLine($"Average number of customers in system: {statistics.AverageCustomersInSystem.ToInvariantCultureString()}");
            WriteLine($"Average time of waiting: {statistics.AverageWaitingTime.ToInvariantCultureString()}");
        }
    }
}
