using System.Collections.Generic;
using System.Drawing;
using ScottPlot;
using ScottPlot.Plottable;

namespace RandomVariableGenerating.Utils
{
    public class HistogramPlotter
    {
        private readonly int[] _inputX;
        private readonly int[] _inputY;
        private readonly ProbabilityMatrix _theoreticalProbabilities;
        private readonly ProbabilityMatrix _empiricalProbabilities;

        public HistogramPlotter(int[] inputX, int[] inputY,
            ProbabilityMatrix theoreticalProbabilities, ProbabilityMatrix empiricalProbabilities)
        {
            _inputX = inputX;
            _inputY = inputY;
            _theoreticalProbabilities = theoreticalProbabilities;
            _empiricalProbabilities = empiricalProbabilities;
        }

        public Plot PlotHistogramX() =>
            AddTitleAndLabels(Plot(_theoreticalProbabilities.XDensity, _empiricalProbabilities.XDensity,
                _inputX));

        public Plot PlotHistogramY() =>
            AddTitleAndLabels(
                Plot(_theoreticalProbabilities.YDensity, _empiricalProbabilities.YDensity, _inputY), "Y");

        private static Plot Plot(IReadOnlyList<double> theoreticalProbabilities, IReadOnlyList<double> empiricalProbabilities, int[] source)
        {
            var plot = new Plot();

            var tColor = Color.Green;
            var eColor = Color.Blue;

            ScatterPlot eLine = null;
            ScatterPlot tLine = null;
            for (var i = 0; i < source.Length; i++)
            {
                var item = source[i];
                eLine = plot.AddLine(item, 0, item, empiricalProbabilities[i], eColor, 5f);
                tLine = plot.AddLine(item, 0, item, theoreticalProbabilities[i], tColor,3f);
            }

            // add legend
            if (eLine != null) 
                eLine.Label = "Empirical probability";
            if (tLine != null) 
                tLine.Label = "Theoretical probability";

            plot.Legend(location: Alignment.UpperLeft);
            return plot;
        }
        
        private static Plot AddTitleAndLabels(Plot plot, string type = "X")
        {
            plot.Title($"{type} Histogram");
            plot.XLabel(type);
            plot.YLabel("Probability");
            return plot;
        }
    }
}
