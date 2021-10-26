using System;
using System.Globalization;
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
            PrintCharacteristics(characteristics);
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
            WriteLine($"Arrival rate: {characteristics.ArrivalRate.ToString(CultureInfo.InvariantCulture)}");
            WriteLine($"Service rate: {characteristics.ServiceRate.ToString(CultureInfo.InvariantCulture)}");
            WriteLine($"Server count: {characteristics.ServerCount.ToString()}");
            WriteLine($"Queue capacity: {characteristics.QueueCapacity.ToString()}");
            WriteLine($"Waiting time: {characteristics.WaitingTime.ToString(CultureInfo.InvariantCulture)}");
        }
    }
}
