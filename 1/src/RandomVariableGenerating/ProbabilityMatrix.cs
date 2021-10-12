using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Ardalis.GuardClauses;

namespace RandomVariableGenerating
{
    public class ProbabilityMatrix
    {
        private readonly double[,] _probabilities;
        private double[] _xProbabilities;
        private double[] _yProbabilities;

        public ProbabilityMatrix(double[,] probabilities)
        {
            _probabilities = Guard.Against.Null(probabilities, nameof(probabilities));
        }

        public double this[int x, int y] => _probabilities[x, y];
        public IReadOnlyList<double> XProbabilities => _xProbabilities ??= GetXProbabilities();
        public IReadOnlyList<double> YProbabilities => _yProbabilities ??= GetYProbabilities();
        public int TotalCount => XProbabilities.Count * YProbabilities.Count;

        private double[] GetXProbabilities()
        {
            var rows = _probabilities.GetLength(0);
            var columns = _probabilities.GetLength(1);
            var xProbabilities = new double[rows];
            for (var i = 0; i< rows ; i++)
            {
                var xProbability = 0d;
                for (var j = 0; j < columns; j++) 
                    xProbability += _probabilities[i, j];
                xProbabilities[i] = xProbability;
            }

            return xProbabilities;
        }
        
        private double[] GetYProbabilities()
        {
            var rows = _probabilities.GetLength(0);
            var columns = _probabilities.GetLength(1);
            var yProbabilities = new double[columns];
            for (var j = 0; j < columns ; j++)
            {
                var yProbability = 0d;
                for (var i = 0; i < rows; i++) 
                    yProbability += _probabilities[i, j];
                yProbabilities[j] = yProbability;
            }

            return yProbabilities;
        }

        public override string ToString()
        {
            var rows = _probabilities.GetLength(0);
            var columns = _probabilities.GetLength(1);
            var sb = new StringBuilder();
            for (var i = 0;  i< rows ; i++)
            {
                for (var j = 0; j < columns; j++)
                    sb.Append($"{_probabilities[i, j].ToString(CultureInfo.InvariantCulture),6} ");
                sb.Remove(sb.Length - 1, 1); // remove last space
                sb.AppendLine();
            }

            return sb.ToString();
        }
        
        public static implicit operator ProbabilityMatrix(double[,] source) => new (source);
        public static implicit operator double[,](ProbabilityMatrix matrix) => matrix._probabilities;
    }
}
