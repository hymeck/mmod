using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using RandomVariableGenerating.Utils;

namespace RandomVariableGenerating.Demo
{
    class Program
    {
        private static void Main(string[] args)
        {
            #region + Input initialization +

            var random = new Random(2021); // deterministic random
            // suppose that values are in ascending order
            var inputX = new[] {4, 8, 9, 11};
            var inputY = new[] {3, 6, 8};
            ProbabilityMatrix probabilities = new[,]
            {
                {0.05, 0.05, 0.10},
                {0.05, 0.15, 0.05},
                {0.10, 0.10, 0.15},
                {0.05, 0.05, 0.10}
            };
            var volume = TryGetVolumeFromArgs(args);
            
            PrintInput(inputX, inputY, probabilities, volume, "Input data");

            #endregion // + Input initialization +

            var variables = RandomVariableGenerator.Generate(random, inputX, inputY, probabilities, volume)
                .ToList();

            #region + Investigations +

            var empiricalProbabilities = BuildEmpiricalProbabilities(inputX, inputY, variables);
            PrintProbabilityMatrix(empiricalProbabilities);
            

            var histogramPlotter = new HistogramPlotter(inputX, inputY, probabilities, empiricalProbabilities);
            var volumeStr = volume.ToString();
            histogramPlotter
                .PlotHistogramX()
                .SaveFig(FullPath($"probabilities_x-{volumeStr}.png"));
            
            histogramPlotter
                .PlotHistogramY()
                .SaveFig(FullPath($"probabilities_y-{volumeStr}.png"));
            
            #endregion // + Investigations +
        }

        private static int TryGetVolumeFromArgs(IReadOnlyList<string> args) =>
            args.Count != 0 && int.TryParse(args[0], out var volume) && volume >= 0
                ? volume
                : 100_000;

        private static ProbabilityMatrix BuildEmpiricalProbabilities(IReadOnlyList<int> inputX, IReadOnlyList<int> inputY, IReadOnlyCollection<(int x, int y)> variables)
        {
            var empiricalProbabilities = new double[inputX.Count, inputY.Count];
            var volume = variables.Count;
            for (var i = 0; i < inputX.Count; i++)
            {
                var currentX = inputX[i];
                for (var j = 0; j < inputY.Count; j++)
                {
                    var currentY = inputY[j];
                    var relativeFrequency = variables.Count(variable => variable == (currentX, currentY));
                    empiricalProbabilities[i, j] = (double) relativeFrequency / volume;
                }
            }

            return empiricalProbabilities;
        }

        private static void PrintInput(IEnumerable<int> x, IEnumerable<int> y, ProbabilityMatrix inputMatrix,
            int volume, string caption)
        {
            Console.WriteLine(caption);
            Console.WriteLine(string.Join(' ', x));
            Console.WriteLine(string.Join(' ', y));
            PrintProbabilityMatrix(inputMatrix);
            Console.WriteLine(volume.ToString());
            Console.WriteLine();
        }

        private static void PrintProbabilityMatrix(ProbabilityMatrix matrix) => Console.Write(matrix.ToString());

        private static string FullPath(string filename) => Path.Combine(Directory.GetCurrentDirectory(), filename);
    }
}
