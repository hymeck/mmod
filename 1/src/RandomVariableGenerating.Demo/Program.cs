using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using RandomVariableGenerating.Extensions;
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
            Console.WriteLine("Empirical probability matrix:");
            PrintProbabilityMatrix(empiricalProbabilities);
            Console.WriteLine();


            var histogramPlotter = new HistogramPlotter(inputX, inputY, probabilities, empiricalProbabilities);
            var volumeStr = volume.ToString();
            histogramPlotter
                .PlotHistogramX()
                .SaveFig(FullPath($"probabilities_x-{volumeStr}.png"));
            
            histogramPlotter
                .PlotHistogramY()
                .SaveFig(FullPath($"probabilities_y-{volumeStr}.png"));
            
            
            var meanXEstimation = probabilities.MeanXPointEstimation(inputX);
            var actualMeanXPointEstimation = empiricalProbabilities.MeanXPointEstimation(inputX);
            Console.Write("Mean X point estimation: ");
            PrintStatistics(meanXEstimation, actualMeanXPointEstimation);
            
            var meanYEstimation = probabilities.MeanYPointEstimation(inputY);
            var actualMeanYPointEstimation = empiricalProbabilities.MeanYPointEstimation(inputY);
            Console.Write("Mean Y point estimation: ");
            PrintStatistics(meanYEstimation, actualMeanYPointEstimation);
            
            var varianceXEstimation = probabilities.VarianceXPointEstimation(inputX);
            var actualVarianceXPointEstimation = empiricalProbabilities.VarianceXPointEstimation(inputX);
            Console.Write("Variance X point estimation: ");
            PrintStatistics(varianceXEstimation, actualVarianceXPointEstimation);
            
            var varianceYEstimation = probabilities.VarianceYPointEstimation(inputY);
            var actualVarianceYPointEstimation = empiricalProbabilities.VarianceYPointEstimation(inputY);
            Console.Write("Variance Y point estimation: ");
            PrintStatistics(varianceYEstimation, actualVarianceYPointEstimation);
            
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
            Console.WriteLine($"X: {string.Join(' ', x)}");
            Console.WriteLine($"Y: {string.Join(' ', y)}");
            Console.WriteLine("Theoretical probability matrix:");
            PrintProbabilityMatrix(inputMatrix);
            Console.WriteLine($"Volume: {volume.ToString()}");
            Console.WriteLine();
        }

        private static void PrintProbabilityMatrix(ProbabilityMatrix matrix) => Console.Write(matrix.ToString());

        private static string FullPath(string filename) => Path.Combine(Directory.GetCurrentDirectory(), filename);

        private static void PrintStatistics(double theoretical, double empirical) =>
            Console.WriteLine(
                $"{theoretical.ToString(CultureInfo.InvariantCulture)} {empirical.ToString(CultureInfo.InvariantCulture)}");
    }
}
